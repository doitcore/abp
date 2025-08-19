import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import cookieParser from 'cookie-parser';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import {environment} from './environments/environment';

// ESM import
import * as oidc from 'openid-client';

//TODO: Remove this line in production
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = "0";

const serverDistFolder = dirname(fileURLToPath(import.meta.url));
const browserDistFolder = resolve(serverDistFolder, '../browser');

const app = express();
const angularApp = new AngularNodeAppEngine();

const ISSUER = new URL('https://localhost:44305/');  // OIDC issuer
const CLIENT_ID = 'MyProjectName_App';
const REDIRECT_URI = 'http://localhost:4200';
const SCOPE = 'offline_access MyProjectName';

const config = await oidc.discovery(ISSUER, CLIENT_ID, /* client_secret */ undefined);
const secureCookie = { httpOnly: false, sameSite: 'lax' as const, secure: environment.production, path: '/' };
const tokenCookie = { ...secureCookie, httpOnly: false, maxAge: 60 * 60 * 24 * 1 }; // 30 days

app.use(cookieParser());

const sessions = new Map<string, { pkce?: string; state?: string; refresh?: string; at?: string }>();

app.get('/authorize', async (_req, res) => {
  const code_verifier  = oidc.randomPKCECodeVerifier();
  const code_challenge = await oidc.calculatePKCECodeChallenge(code_verifier);
  const state = oidc.randomState();

  const sid = crypto.randomUUID();
  sessions.set(sid, { pkce: code_verifier, state });
  res.cookie('sid', sid, secureCookie);

  const url = oidc.buildAuthorizationUrl(config, {
    redirect_uri: REDIRECT_URI,
    scope: SCOPE,
    code_challenge,
    code_challenge_method: 'S256',
    state,
  });
  res.redirect(url.toString());
});

app.get('/', async (req, res, next) => {
  try {
    const { code, state } = req.query as any;
    if (!code || !state) return next();

    const sid  = req.cookies.sid;
    const sess = sid && sessions.get(sid);
    if (!sess || state !== sess.state) return res.status(400).send('invalid state');

    const tokenEndpoint = config.serverMetadata().token_endpoint!;
    const body = new URLSearchParams({
      grant_type: 'authorization_code',
      code: String(code),
      redirect_uri: 'http://localhost:4200',
      code_verifier: sess.pkce!,
      client_id: CLIENT_ID, // public client
    });

    const resp = await fetch(tokenEndpoint, {
      method: 'POST',
      headers: { 'content-type': 'application/x-www-form-urlencoded' },
      body,
    });

    if (!resp.ok) {
      const errTxt = await resp.text();
      console.error('token error:', resp.status, errTxt);
      return res.status(500).send('token error');
    }

    const tokens = await resp.json();
    console.log(tokens);

    const expiresInSec =
      Number(tokens.expires_in ?? tokens.expiresIn ?? 3600);
    const skewSec = 60;
    const accessExpiresAt = new Date(
      Date.now() + Math.max(0, expiresInSec - skewSec) * 1000
    );

    sessions.set(sid, { ...sess, at: tokens.access_token, refresh: tokens.refresh_token });
    res.cookie('access_token', tokens.access_token, tokenCookie);
    res.cookie('refresh_token', tokens.refresh_token, secureCookie);
    res.cookie('expires_at', String(accessExpiresAt.getTime()), tokenCookie);
    return res.redirect('/');
  } catch (e) {
    console.error('OIDC error:', e);
    return res.status(500).send('oidc error');
  }
});

/**
 * Serve static files from /browser
 */
app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

/**
 * Handle all other requests by rendering the Angular application.
 */
app.use((req, res, next) => {
  angularApp
    .handle(req)
    .then(response => (response ? writeResponseToNodeResponse(response, res) : next()))
    .catch(next);
});

/**
 * Start the server if this module is the main entry point.
 * The server listens on the port defined by the `PORT` environment variable, or defaults to 4000.
 */
if (isMainModule(import.meta.url)) {
  const port = process.env['PORT'] || 4200;
  app.listen(port, () => {
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

/**
 * Request handler used by the Angular CLI (for dev-server and during build) or Firebase Cloud Functions.
 */
export const reqHandler = createNodeRequestHandler(app);
