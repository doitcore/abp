/// <reference lib="webworker" />

// Message types for type safety
interface TokenMessage {
  action: 'set' | 'remove' | 'clear' | 'get';
  key?: string;
  value?: string;
}

interface TokenResponse {
  action: 'set' | 'remove' | 'clear' | 'get';
  key?: string;
  value?: string | null;
}

declare const self: SharedWorkerGlobalScope;

const tokenStore = new Map<string, string>();
const ports = new Set<MessagePort>();

// Broadcast message to all ports except the sender
// Automatically cleans up dead ports
function broadcastToOtherPorts(senderPort: MessagePort, message: TokenResponse): void {
  const deadPorts: MessagePort[] = [];

  ports.forEach(p => {
    if (p !== senderPort) {
      try {
        p.postMessage(message);
      } catch (error) {
        // Port is dead/closed, mark for removal
        console.log('Dead port detected, removing...');
        deadPorts.push(p);
      }
    }
  });

  // Clean up dead ports
  deadPorts.forEach(p => ports.delete(p));

  if (deadPorts.length > 0) {
    console.log('Cleaned up dead ports. Total ports:', ports.size);
  }
}

self.onconnect = (event: MessageEvent) => {
  const port = event.ports[0];
  ports.add(port);
  console.log('Port connected. Total ports:', ports.size);

  // Clean up port when it's closed
  port.addEventListener('messageerror', () => {
    ports.delete(port);
    console.log('Port disconnected (error). Total ports:', ports.size);
  });

  port.onmessage = (e: MessageEvent<TokenMessage>) => {
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
          console.log('remove', key);
          tokenStore.delete(key);
          broadcastToOtherPorts(port, { action: 'remove', key });
        }
        break;

      case 'clear':
        console.log('clear user');
        tokenStore.clear();
        broadcastToOtherPorts(port, { action: 'clear' });
        break;

      case 'get':
        console.log('get user');
        if (key) {
          const value = tokenStore.get(key) ?? null;
          port.postMessage({ action: 'get', key, value } as TokenResponse);
        }
        break;

      default:
        console.warn('Unknown action:', action);
    }
  };

  port.start();
};

export {};
