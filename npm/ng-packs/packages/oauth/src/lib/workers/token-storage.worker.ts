/// <reference no-default-lib="true"/>
/// <reference lib="es2020" />
/// <reference lib="webworker" />

declare const self: SharedWorkerGlobalScope;

interface TokenMessage {
  action: 'set' | 'remove' | 'clear' | 'get';
  key?: string;
  value?: string;
}

const tokenStore = new Map<string, string>();
const ports = new Set<MessagePort>();

self.onconnect = (event: MessageEvent) => {
  const port = event.ports[0];
  ports.add(port);

  port.onmessage = (e: MessageEvent<TokenMessage>) => {
    const { action, key, value } = e.data;
    switch (action) {
      case 'set':
        if (key && value !== undefined) {
          tokenStore.set(key, value);
          ports.forEach(p => {
            if (p !== port) {
              p.postMessage({ action: 'set', key, value });
            }
          });
        }
        break;
      case 'remove':
        if (key) {
          tokenStore.delete(key);
          ports.forEach(p => {
            if (p !== port) {
              p.postMessage({ action: 'remove', key });
            }
          });
        }
        break;
      case 'clear':
        tokenStore.clear();
        ports.forEach(p => {
          if (p !== port) {
            p.postMessage({ action: 'clear' });
          }
        });
        break;
      case 'get':
        if (key) {
          const value = tokenStore.get(key) ?? null;
          port.postMessage({ action: 'get', key, value });
        }
        break;
    }
  };

  port.start();
};

export {};
