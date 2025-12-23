function initWatari(serverPort) {
  window.watari = {
    invoke: async function (method, ...args) {
      const response = await fetch(`http://localhost:${serverPort}/invoke`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ method, args }),
      });
      if (response.status === 204) {
        return;
      }
      if (!response.ok) {
        throw new Error(await response.text());
      }
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
    drop_zone: function (elementId, callback) {
      const element = document.getElementById(elementId);
      if (!element) {
        throw new Error("Element not found for drop zone");
      }
      if (this.callbacks[elementId]) {
        throw new Error("Drop zone already registered for this element");
      }
      const callbackId = Date.now().toString();
      this.callbacks[callbackId] = callback;
      const allowed =
        element.getAttribute("data-watari-allowed-extensions") || "";
      window.webkit.messageHandlers.setDropZone.postMessage({
        callbackId: callbackId,
        element: elementId,
        allowedExtensions: allowed,
      });
      return () => {
        delete this.callbacks[callbackId];
        window.webkit.messageHandlers.removeDropZone.postMessage({
          callbackId: callbackId,
        });
      };
    },
    openFileDialog: function (allowedExtensions) {
      return new Promise((resolve) => {
        const callbackId = Date.now().toString();
        this.callbacks[callbackId] = resolve;
        window.webkit.messageHandlers.openFileDialog.postMessage({
          callbackId: callbackId,
          allowedExtensions: allowedExtensions,
        });
      });
    },
    _checkDropZone: function (elementId, x, y) {
      const el = document.getElementById(elementId);
      return document.elementFromPoint(x, y) === el;
    },
    _updateDropZoneClass: function (elementId, isOver, allowed) {
      const el = document.getElementById(elementId);
      if (!el) {
        return;
      }
      el.classList.remove("--watari-drop", "--watari-drop-not-allowed");
      if (isOver) {
        el.classList.add(
          allowed ? "--watari-drop" : "--watari-drop-not-allowed"
        );
      }
    },
    _validateExtensions: function (allowed, paths) {
      if (!allowed || allowed.length === 0) {
        return true;
      }
      const allowedExts = allowed.split(",").map((e) => e.trim().toLowerCase());
      for (const path of paths) {
        const ext = path.split(".").pop().toLowerCase();
        if (!allowedExts.includes(ext)) return false;
      }
      return true;
    },
    _checkAndValidateDropZone: function (elementId, x, y, allowed, pathsJson) {
      const paths = JSON.parse(pathsJson);
      const isOver = this._checkDropZone(elementId, x, y);
      const allowedFiles = this._validateExtensions(allowed, paths);
      this._updateDropZoneClass(elementId, isOver ? 1 : 0, allowedFiles ? 1 : 0);
      return allowedFiles;
    },
    _handleDrop: function (elementId, x, y, pathsJson, callbackId) {
      const paths = JSON.parse(pathsJson);
      const el = document.getElementById(elementId);
      el.classList.remove("--watari-drop", "--watari-drop-not-allowed");
      if (this._checkDropZone(elementId, x, y)) {
        this.callbacks[callbackId](paths);
      }
    },
  };
  window.watari._connectEvents();
}
