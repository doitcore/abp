// src/lib/workers/token-storage.worker.ts
var tokenStore = /* @__PURE__ */ new Map();
var ports = /* @__PURE__ */ new Set();
function broadcastToOtherPorts(senderPort, message) {
  const deadPorts = [];
  ports.forEach((p) => {
    if (p !== senderPort) {
      try {
        p.postMessage(message);
      } catch (error) {
        console.log("Dead port detected, removing...");
        deadPorts.push(p);
      }
    }
  });
  deadPorts.forEach((p) => ports.delete(p));
  if (deadPorts.length > 0) {
    console.log("Cleaned up dead ports. Total ports:", ports.size);
  }
}
self.onconnect = (event) => {
  const port = event.ports[0];
  ports.add(port);
  console.log("Port connected. Total ports:", ports.size);
  port.addEventListener("messageerror", () => {
    ports.delete(port);
    console.log("Port disconnected (error). Total ports:", ports.size);
  });
  port.onmessage = (e) => {
    const { action, key, value } = e.data;
    switch (action) {
      case "set":
        if (key && value !== void 0) {
          tokenStore.set(key, value);
          broadcastToOtherPorts(port, { action: "set", key, value });
        }
        break;
      case "remove":
        if (key) {
          console.log("remove", key);
          tokenStore.delete(key);
          broadcastToOtherPorts(port, { action: "remove", key });
        }
        break;
      case "clear":
        console.log("clear user");
        tokenStore.clear();
        broadcastToOtherPorts(port, { action: "clear" });
        break;
      case "get":
        console.log("get user");
        if (key) {
          const value2 = tokenStore.get(key) ?? null;
          port.postMessage({ action: "get", key, value: value2 });
        }
        break;
      default:
        console.warn("Unknown action:", action);
    }
  };
  port.start();
};
