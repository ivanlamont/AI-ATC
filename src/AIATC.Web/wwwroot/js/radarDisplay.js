window.radarDisplay = {
    canvases: new Map(),
    backgrounds: new Map(),

    initialize: function (canvasElement) {
        const canvas = canvasElement;
        const ctx = canvas.getContext('2d');
        this.canvases.set(canvas, ctx);
        this.backgrounds.delete(canvas);

        // Disable right-click context menu on canvas
        canvas.addEventListener('contextmenu', (e) => e.preventDefault());
    },

    clearCanvas: function (canvasElement) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.fillStyle = '#001100';
        ctx.fillRect(0, 0, canvasElement.width, canvasElement.height);
    },

    cacheBackground: function (canvasElement) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        let backgroundCanvas = this.backgrounds.get(canvasElement);
        if (!backgroundCanvas) {
            backgroundCanvas = document.createElement('canvas');
            this.backgrounds.set(canvasElement, backgroundCanvas);
        }

        backgroundCanvas.width = canvasElement.width;
        backgroundCanvas.height = canvasElement.height;

        const bgCtx = backgroundCanvas.getContext('2d');
        bgCtx.clearRect(0, 0, backgroundCanvas.width, backgroundCanvas.height);
        bgCtx.drawImage(canvasElement, 0, 0);
    },

    drawCachedBackground: function (canvasElement) {
        const ctx = this.canvases.get(canvasElement);
        const backgroundCanvas = this.backgrounds.get(canvasElement);
        if (!ctx || !backgroundCanvas) return false;

        ctx.clearRect(0, 0, canvasElement.width, canvasElement.height);
        ctx.drawImage(backgroundCanvas, 0, 0);
        return true;
    },

    drawCircle: function (canvasElement, x, y, radius, color, lineWidth) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.strokeStyle = color;
        ctx.lineWidth = lineWidth;
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, 2 * Math.PI);
        ctx.stroke();
    },

    drawLine: function (canvasElement, x1, y1, x2, y2, color, lineWidth, dashed = false) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.strokeStyle = color;
        ctx.lineWidth = lineWidth;

        if (dashed) {
            ctx.setLineDash([5, 5]);
        } else {
            ctx.setLineDash([]);
        }

        ctx.beginPath();
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.stroke();

        ctx.setLineDash([]);
    },

    drawRect: function (canvasElement, x, y, width, height, color) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.fillStyle = color;
        ctx.fillRect(x, y, width, height);
    },

    drawTriangle: function (canvasElement, x, y, size, color) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.moveTo(x, y - size);
        ctx.lineTo(x - size, y + size);
        ctx.lineTo(x + size, y + size);
        ctx.closePath();
        ctx.fill();
    },

    drawChevron: function (canvasElement, x, y, size, headingRad, color) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.save();
        ctx.translate(x, y);
        ctx.rotate(-headingRad); // Negative because canvas Y is inverted

        ctx.strokeStyle = color;
        ctx.fillStyle = color;
        ctx.lineWidth = 2;

        // Draw chevron shape pointing up (north)
        ctx.beginPath();
        ctx.moveTo(0, -size);           // Tip
        ctx.lineTo(-size * 0.6, size);   // Left wing
        ctx.lineTo(0, size * 0.5);       // Center back
        ctx.lineTo(size * 0.6, size);    // Right wing
        ctx.lineTo(0, -size);            // Back to tip
        ctx.closePath();
        ctx.fill();

        ctx.restore();
    },

    drawText: function (canvasElement, text, x, y, color, font) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.fillStyle = color;
        ctx.font = font;
        ctx.textAlign = 'left';
        ctx.textBaseline = 'middle';
        ctx.fillText(text, x, y);
    },

    drawTextBox: function (canvasElement, x, y, lines, bgColor, textColor, font) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx) return;

        ctx.font = font;

        // Measure text to determine box size
        const lineHeight = 14;
        const padding = 4;
        let maxWidth = 0;

        for (const line of lines) {
            const metrics = ctx.measureText(line);
            maxWidth = Math.max(maxWidth, metrics.width);
        }

        const boxWidth = maxWidth + padding * 2;
        const boxHeight = lines.length * lineHeight + padding * 2;

        // Draw background box
        ctx.fillStyle = bgColor;
        ctx.fillRect(x, y, boxWidth, boxHeight);

        // Draw border
        ctx.strokeStyle = textColor;
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, boxWidth, boxHeight);

        // Draw text lines
        ctx.fillStyle = textColor;
        ctx.textAlign = 'left';
        ctx.textBaseline = 'top';

        for (let i = 0; i < lines.length; i++) {
            ctx.fillText(lines[i], x + padding, y + padding + i * lineHeight);
        }
    },

    drawAircraftBatch: function (canvasElement, aircraftItems) {
        const ctx = this.canvases.get(canvasElement);
        if (!ctx || !Array.isArray(aircraftItems)) return;

        for (const item of aircraftItems) {
            if (!item) continue;

            // Aircraft chevron
            ctx.save();
            ctx.translate(item.x, item.y);
            ctx.rotate(-item.headingRad);
            ctx.strokeStyle = item.color;
            ctx.fillStyle = item.color;
            ctx.lineWidth = 2;
            const size = 8;
            ctx.beginPath();
            ctx.moveTo(0, -size);
            ctx.lineTo(-size * 0.6, size);
            ctx.lineTo(0, size * 0.5);
            ctx.lineTo(size * 0.6, size);
            ctx.closePath();
            ctx.fill();
            ctx.restore();

            // Target heading line
            if (typeof item.targetHeadingRad === 'number') {
                const lineLength = 30;
                const endX = item.x + lineLength * Math.cos(item.targetHeadingRad);
                const endY = item.y - lineLength * Math.sin(item.targetHeadingRad);
                ctx.strokeStyle = '#ffff00';
                ctx.lineWidth = 1;
                ctx.setLineDash([5, 5]);
                ctx.beginPath();
                ctx.moveTo(item.x, item.y);
                ctx.lineTo(endX, endY);
                ctx.stroke();
                ctx.setLineDash([]);
            }

            // Data tag
            const tagX = item.x + 15;
            const tagY = item.y - 10;
            const lines = [item.callsign || '', item.altSpd || ''];
            const font = '11px monospace';
            const lineHeight = 14;
            const padding = 4;

            ctx.font = font;
            let maxWidth = 0;
            for (const line of lines) {
                const metrics = ctx.measureText(line);
                maxWidth = Math.max(maxWidth, metrics.width);
            }

            const boxWidth = maxWidth + padding * 2;
            const boxHeight = lines.length * lineHeight + padding * 2;

            ctx.fillStyle = item.isSelected ? 'rgba(0, 50, 0, 0.8)' : 'rgba(0, 30, 0, 0.7)';
            ctx.fillRect(tagX, tagY, boxWidth, boxHeight);

            ctx.strokeStyle = item.isSelected ? '#00ff00' : '#00cc00';
            ctx.lineWidth = 1;
            ctx.strokeRect(tagX, tagY, boxWidth, boxHeight);

            ctx.fillStyle = item.isSelected ? '#00ff00' : '#00cc00';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            for (let i = 0; i < lines.length; i++) {
                ctx.fillText(lines[i], tagX + padding, tagY + padding + i * lineHeight);
            }
        }
    }
};
