// ESM SharedWorker

const tokenStore = new Map();
const ports = new Set();

self.onconnect = (event) => {
  const port = event.ports[0];
  ports.add(port);
  port.onmessage = (e) => {
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
