// ESM SharedWorker

const tokenStore = new Map();
const ports = new Set();

// Broadcast message to all ports except the sender and remove dead ports
function broadcastToOtherPorts(senderPort, message) {
  const deadPorts = [];

  ports.forEach(p => {
    if (p !== senderPort) {
      try {
        p.postMessage(message);
      } catch (error) {
        console.log('Dead port detected, removing...');
        deadPorts.push(p);
      }
    }
  });

  // Clean up dead ports
  deadPorts.forEach(p => ports.delete(p));
}

self.onconnect = (event) => {
  const port = event.ports[0];
  ports.add(port);

  // Clean up port when it's closed
  port.addEventListener('messageerror', () => {
    ports.delete(port);
    console.log('Port disconnected (error). Total ports:', ports.size);
  });

  port.onmessage = (e) => {
    const { action, key, value } = e.data;
    switch (action) {
      case 'set':
        if (key && value !== undefined) {
          tokenStore.set(key, value);
          broadcastToOtherPorts(port, { action: 'set', key, value });
        }
        break;
      case 'remove':
        if (key) {
          tokenStore.delete(key);
          broadcastToOtherPorts(port, { action: 'remove', key });
        }
        break;
      case 'clear':
        tokenStore.clear();
        broadcastToOtherPorts(port, { action: 'clear' });
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
