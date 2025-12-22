function initWatari(serverPort) {
  window.watari = {
    invoke: async function (method, ...args) {
      const response = await fetch(`http://localhost:${serverPort}/invoke`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ method, args }),
      });
      return await response.json();
    },
    _eventHandlers: {},
    on: function (event, handler) {
      if (!this._eventHandlers[event]) {
        this._eventHandlers[event] = [];
      }
      this._eventHandlers[event].push(handler);
    },
    off: function (event, handler) {
      if (this._eventHandlers[event]) {
        this._eventHandlers[event] = this._eventHandlers[event].filter(
          (h) => h !== handler
        );
      }
    },
    _ws: null,
    _connectEvents: function () {
      this._ws = new WebSocket(`ws://localhost:${serverPort}/events`);
      this._ws.onmessage = (event) => {
        const msg = JSON.parse(event.data);
        const handlers = this._eventHandlers[msg.event] || [];
        handlers.forEach((h) => h(msg.data));
      };
      this._ws.onclose = () => {
        setTimeout(() => this._connectEvents(), 1000);
      };
    },
    callbacks: {},
    drop_zone: function (element, callback) {
      if (!element.id || document.getElementById(element.id) !== element) {
        throw new Error("Element must have a unique id");
      }
      const id = Date.now().toString();
      this.callbacks[id] = callback;
      const allowed =
        element.getAttribute("data-watari-allowed-extensions") || "";
      window.webkit.messageHandlers.setDropZone.postMessage({
        callbackId: id,
        element: "#" + element.id,
        allowedExtensions: allowed,
      });
      return () => {
        delete this.callbacks[id];
        window.webkit.messageHandlers.removeDropZone.postMessage({
          callbackId: id,
        });
      };
    },
    _checkDropZone: function (selector, x, y) {
      const el = document.querySelector(selector);
      return document.elementFromPoint(x, y) === el;
    },
    _updateDropZoneClass: function (selector, isOver, allowed) {
      const el = document.querySelector(selector);
      if (el) {
        el.classList.remove("--watari-drop", "--watari-drop-not-allowed");
        if (isOver) {
          el.classList.add(
            allowed ? "--watari-drop" : "--watari-drop-not-allowed"
          );
        }
      }
    },
    _clearDropZoneClass: function (selector) {
      const el = document.querySelector(selector);
      if (el) {
        el.classList.remove("--watari-drop", "--watari-drop-not-allowed");
      }
    },
  };
  window.watari._connectEvents();
}
