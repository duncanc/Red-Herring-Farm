
jQuery(function($) {

    var testCanvas = document.createElement("CANVAS");
    var testCtx;
    
    var contextName;
    
    try {
        contextName = "webgl";
        testCtx = testCanvas.getContext(contextName);
        if (!testCtx) {
            throw "no webgl context";
        }
    }
    catch (e) {
        try {
            contextName = "experimental-webgl";
            testCtx = testCanvas.getContext(contextName);
            if (!testCtx) {
                throw "no experimental-webgl context";
            }
        }
        catch (e) {
            contextName = null;
        }
    }
    testCtx = testCanvas = null;

    // Special event for image load events
    // Needed because some browsers does not trigger the event on cached images.
    // Mini-plugin by Paul Irish (MIT License)
    if (typeof $.event.special.load === 'undefined') {
        $.event.special.load = {
            add: function(hollaback) {
                if (this.nodeType === 1 && this.tagName.toLowerCase() === 'img' && this.src !== '') {
                    // Image is already complete, fire the hollaback (fixes browser issues were cached
                    // images isn't triggering the load event)
                    if (this.complete || this.readyState === 4) {
                        hollaback.handler.apply(this);
                    }
                    // Check if data URI images is supported, fire 'error' event if not
                    else if (this.readyState === 'uninitialized' && this.src.indexOf('data:') === 0) {
                        $(this).trigger('error');
                    }
                    else {
                        $(this).bind('load', hollaback.handler);
                    }
                }
            }
        };
    }

    ags.WebGLRenderer = function(engine, canvas) {
        var renderer = this;
        var mouse = engine.mouse;
        var mouseCursorSprite = new ags.WebGLRenderer.Sprite();
        mouseCursorSprite.visible = false;
        this.mouseCursorSprite = mouseCursorSprite;
        $(canvas).mousemove(function(e) {
            mouse.nextOnScreen = true;
            var nx, ny;
            if (typeof e.offsetX !== 'undefined') {
                nx = e.offsetX;
                ny = e.offsetY;
            }
            else {
                nx = e.layerX;
                ny = e.layerY;
            }
            nx = Math.floor((nx / $(canvas).width()) * canvas.width);
            ny = Math.floor((ny / $(canvas).height()) * canvas.height);
            mouse.nextX = nx;
            mouse.nextY = ny;
            mouseCursorSprite.visible = true;
            mouseCursorSprite.x = mouse.nextX;
            mouseCursorSprite.y = mouse.nextY;
        });
        $(canvas).mouseout(function(e) {
            $(engine).trigger('mouseLeft');
            mouse.nextOnScreen = false;
            mouseCursorSprite.visible = false;
        });
        $(canvas).mousedown(function(e) {
            switch(e.which) {
                case 1: $(engine).trigger('mouseDown', [1 /* LEFT */]); break;
                case 2: $(engine).trigger('mouseDown', [3 /* MIDDLE */]); break;
                case 3: $(engine).trigger('mouseDown', [2 /* RIGHT */]); break;
            }
            e.preventDefault();
            return false;
        });
        $(canvas).mouseup(function(e) {
            switch(e.which) {
                case 1: $(engine).trigger('mouseUp', [1 /* LEFT */]); break;
                case 2: $(engine).trigger('mouseUp', [3 /* MIDDLE */]); break;
                case 3: $(engine).trigger('mouseUp', [2 /* RIGHT */]); break;
            }            
            e.preventDefault();
            return false;
        });
        canvas.oncontextmenu = function(e) {
            e.preventDefault();
            return false;
        };
        this.engine = engine;
        this.canvas = canvas;
        this.ctx = canvas.getContext('webgl');
        var scene = [];
        this.scene = scene;
        this.overlays = [];
        var mouseOver = null;
        $(engine).bind('mouseLeft', function() {
            mouseOver = null;
        });
        $(engine).bind('frame', function() {
            var i;
            if (mouse.nextOnScreen) {
                var mx = mouse.nextX, my = mouse.nextY;
                var newMouseOver = null;
                for (i = scene.length-1; i >= 0; i--) {
                    var spr = scene[i];
                    if (!spr.visible || !spr.clickable) continue;
                    var img = renderer.numberedImages[spr.image];
                    var represents = spr.represents;
                    if (img && represents) {
                        var x = spr.x + Math.floor(spr.multX * img.w) + spr.offX, y = spr.y + Math.floor(spr.multY * img.h) + spr.offY;
                        if (mx >= x && my >= y) {
                            var ix = mx - x, iy = my - y;
                            if (ix < img.w && iy < img.h && img.data.data[((iy * img.w) << 2) + (ix << 2) + 3] >= 128) {
                                newMouseOver = spr.represents;
                                break;
                            }
                        }
                    }
                }
                if (newMouseOver === null) {
                    var hsmask = renderer.hotspotMask;
                    if (hsmask) {
                        var offset = (4 * hsmask.width * my) + (4 * mx);
                        var r = hsmask.data[offset], g = hsmask.data[offset+1], b = hsmask.data[offset+2];
                        var hotspotNumber;
                        switch(((r & 0xC0) << 16) | ((g & 0xC0) << 8) | (b & 0xC0)) {
                            case 0x000000: hotspotNumber = 0; break;
                            case 0x000080: hotspotNumber = 1; break;
                            case 0x008000: hotspotNumber = 2; break;
                            case 0x008080: hotspotNumber = 3; break;
                            case 0x800000: hotspotNumber = 4; break;
                            case 0x800080: hotspotNumber = 5; break;
                            case 0x804000: hotspotNumber = 6; break; // not a mistake
                            case 0x808080: hotspotNumber = 7; break;
                            case 0x404040: hotspotNumber = 8; break;
                            case 0x4040C0: hotspotNumber = 9; break;
                            case 0x40C040: hotspotNumber = 10; break;
                            case 0x40C0C0: hotspotNumber = 11; break;
                            case 0xC04040: hotspotNumber = 12; break;
                            case 0xC040C0: hotspotNumber = 13; break;
                            case 0xC0C040: hotspotNumber = 14; break;
                            case 0xC0C0C0: hotspotNumber = 15; break;
                            // TODO: support more than 16 hotspots
                            default: hotspotNumber = 0; break;
                        }
                        if (hotspotNumber !== 0) {
                            newMouseOver = engine.game.hotspot[hotspotNumber];
                        }
                    }
                }
                if (newMouseOver !== mouseOver) {
                    mouseOver = newMouseOver;
                    $(engine).trigger('mouseMovedOver', [mouseOver]);
                }
            }
            renderer.render();
        });
        this.numberedImageSets = [];
        this.numberedImages = [];
        this.walkbehindLayers = [];
        this.guis = [];
        this.tempCanvas = document.createElement("CANVAS");
    };
    ags.WebGLRenderer.prototype = {
        engine: null, // set in constructor
        ctx: null, // set in constructor
        numberedImages: null,
        background: null,
        hotspotMask: null,
        scene: null,
        tempCanvas: null,
        overlays: null,
        viewportX: 0,
        viewportY: 0,
        fadeAlpha: 1,
        walkbehindLayers: null,
        fadeRgb: 'rgba(0,0,0,',
        mouseCursorSprite: null,
        mustRedrawWalkbehinds: false,
        getMouseCursorSprite: function() {
            return this.mouseCursorSprite;
        },
        setHotspotMask: function(img) {
            var tempCanvas = document.createElement("CANVAS");
            tempCanvas.width = img.pixelWidth;
            tempCanvas.height = img.pixelHeight;
            var tempCtx = tempCanvas.getContext('2d');
            tempCtx.globalCompositeOperation = 'copy';
            tempCtx.drawImage(img, 0, 0);
            this.hotspotMask = tempCtx.getImageData(0, 0, img.pixelWidth, img.pixelHeight);
        },
        render: function() {
            var ctx = this.ctx;
            
            // background 
            var background = this.background;
            if (background) {
                ctx.drawImage(background, -this.viewportX, -this.viewportY);
            }
            
            if (this.mustRedrawWalkbehinds) {
                this._redrawWalkbehinds();
                this.mustRedrawWalkbehinds = false;
            }
            
            // setup for walkbehind masks
            var renderTo = ctx;
            var wbLayers = this.walkbehindLayers;
            var wbCount = wbLayers.length;
            var wb_i = 1;
            while (wb_i < wbCount && wbLayers[wb_i].walkbehind.baseline <= 0) {
                wb_i++;
            }
            var tempCanvas, tempCtx;
            var i, j;
            var img;
            // scene sprites
            for (i = 0; i < this.scene.length; i++) {
                var sprite = this.scene[i];
                if (sprite.visible) {
                    if (wb_i < wbCount) {
                        var currentLayer = wbLayers[wb_i];
                        var sprite_baseline = sprite.baseline || sprite.y;
                        if (currentLayer.walkbehind.baseline < sprite_baseline) {
                            if (tempCanvas) {
                                tempCtx.save();
                                tempCtx.globalCompositeOperation = 'destination-out';
                                tempCtx.drawImage(currentLayer.render, 0, 0);
                                tempCtx.restore();
                                ctx.drawImage(tempCanvas, 0, 0);
                                tempCtx.clearRect(0, 0, tempCanvas.width, tempCanvas.height);
                            }
                            do {
                                wb_i++;
                            }
                            while (wb_i < wbCount && wbLayers[wb_i].walkbehind.baseline < sprite_baseline);
                            if (wb_i === wbCount) {
                                currentLayer = null;
                                tempCanvas = null;
                                tempCtx = null;
                                renderTo = ctx;
                            }
                            else {
                                currentLayer = wbLayers[wb_i];
                            }
                        }
                        if (currentLayer) {
                            if (!tempCanvas) {
                                tempCanvas = this.tempCanvas;
                                tempCanvas.width = background.width;
                                tempCanvas.height = background.height;
                                tempCtx = tempCanvas.getContext("2d");
                                renderTo = tempCtx;
                            }
                        }
                    }
                    if (sprite.image === -1) {
                        renderTo.fillStyle = 'rgba(255,0,0,' + sprite.alpha + ')';
                        renderTo.fillRect(sprite.x - 5, sprite.y - 20, 10, 20);
                    }
                    else {
                        img = this.numberedImages[sprite.image];
                        renderTo.save();
                        renderTo.globalAlpha = sprite.alpha;
                        var tx = sprite.x + Math.floor(sprite.multX * img.w) + sprite.offX;
                        var ty = sprite.y + Math.floor(sprite.multY * img.h) + sprite.offY;
                        try {
                            renderTo.translate(tx,ty);
                        }
                        catch (e) {
                            document.title = sprite.x + ',' + sprite.multX + ',' + img.w
                                + ':' + sprite.y + ',' + sprite.multY + ',' + img.h;
                        }
                        if (sprite.flipped) {
                            renderTo.translate(img.w, 0);
                            renderTo.scale(-1, 1);
                        }
                        renderTo.drawImage(img.image, img.x, img.y, img.w, img.h, 0, 0, img.w, img.h);
                        renderTo.restore();
                    }
                }
            }
            if (tempCanvas) {
                ctx.drawImage(tempCanvas, 0, 0);
            }
            // GUIs
            var guis = this.guis;
            for (i = 0; i < guis.length; i++) {
                var gui = guis[i].gui;
                if (!gui.visible) {
                    continue;
                }
                var guiDef = gui.def;
                ctx.save();
                ctx.translate(guiDef.left, guiDef.top);
                if (guiDef.backgroundColor !== 0) {
                    ctx.fillStyle = this.engine.getColorRgbString(guiDef.backgroundColor);
                    ctx.fillRect(0, 0, guiDef.width, guiDef.height);
                }
                for (j = 0; j < gui.controls.length; j++) {
                    var control = gui.controls[j];
                    var controlDef = control.def;
                    if (!this.engine.interactive) {
                        continue;
                    }
                    ctx.save();
                    ctx.translate(controlDef.left, controlDef.top);
                    switch(controlDef.type) {
                        case 'label':
                            var font = this.engine.game.fonts[controlDef.font];
                            var text = control.text;
                            var render;
                            if (typeof control._cachetext !== 'undefined' && text === control._cachetext) {
                                render = control._cacherender;
                            }
                            else {
                                render = font.renderString(control.text, controlDef.textColor);
                                control._cachetext = text;
                                control._cacherender = render;
                            }
                            ctx.drawImage(render,
                                Math.floor((controlDef.width - render.width)/2),
                                0);
                            break;
                    }
                    ctx.restore();
                }
                ctx.restore();
            }
            // Overlays
            for (i = 0; i < this.overlays.length; i++) {
                var overlay = this.overlays[i];
                switch(overlay.type) {
                    case 'text':
                        ctx.drawImage(overlay.render, overlay.x, overlay.y);
                        break;
                }
            }
            // Screen fade layer
            var fadeAlpha = this.fadeAlpha;
            if (fadeAlpha > 0) {
                ctx.fillStyle = this.fadeRgb + fadeAlpha + ')';
                ctx.fillRect(0, 0, 320, 240);
            }
            var mouseCursorSprite = this.mouseCursorSprite;
            if (mouseCursorSprite.visible) {
                img = this.numberedImages[mouseCursorSprite.image];
                ctx.save();
                ctx.globalAlpha = mouseCursorSprite.alpha;
                ctx.translate(
                    mouseCursorSprite.x + Math.floor(mouseCursorSprite.multX * img.w) + mouseCursorSprite.offX,
                    mouseCursorSprite.y + Math.floor(mouseCursorSprite.multY * img.h) + mouseCursorSprite.offY);
                ctx.drawImage(img.image, img.x, img.y, img.w, img.h, 0, 0, img.w, img.h);
                ctx.restore();
            }
        },
        setBackground: function(bg) {
            this.background = bg;
        },
        setWalkbehindBase: function() {
            this.redrawWalkbehinds();
        },
        setWalkbehinds: function(walkbehinds) {
            var newLayers = [];
            newLayers[0] = null;
            var i;
            for (i = 1; i < walkbehinds.length; i++) {
                var renderCanvas = document.createElement("CANVAS");
                renderCanvas.width = this.background.width;
                renderCanvas.height = this.background.height;
                newLayers[i] = {walkbehind:walkbehinds[i], render:renderCanvas, renderCtx:renderCanvas.getContext('2d')};
            }
            this.walkbehindLayers = newLayers;
            this.redrawWalkbehinds();
        },
        redrawWalkbehinds: function() {
            this.mustRedrawWalkbehinds = true;
        },
        _redrawWalkbehinds: function() {
            var layers = this.walkbehindLayers;
            // first sort into increasing baseline
            var i, len = layers.length;
            for (i = 2; i < len; ) {
                var currentLayer = layers[i], previousLayer = layers[i-1];
                if (currentLayer.walkbehind.baseline < previousLayer.walkbehind.baseline) {
                    currentLayer.changed = true;
                    previousLayer.changed = true;
                    layers[i] = previousLayer;
                    layers[i-1] = currentLayer;
                    if (i > 2) i--;
                }
                else {
                    i++;
                }
            }
            // now render out the layers cumulatively, from the nearest
            var lastCanvas;
            for (i = len-1; i >= 1; i--) {
                var layer = layers[i];
                // if baseline is 0, ignore this layer anyway
                if (layer.walkbehind.baseline <= 0) {
                    break;
                }
                var renderCtx = layer.renderCtx;
                renderCtx.globalCompositeOperation = 'copy';
                if (lastCanvas) {
                    renderCtx.drawImage(lastCanvas, 0, 0);
                    renderCtx.globalCompositeOperation = 'source-over';
                }
                renderCtx.drawImage(layers[i].walkbehind.image, 0, 0);
                lastCanvas = layer.render;
            }
        },
        clearScene: function() {
            var scene = this.scene;
            var i;
            for (i = 0; i < scene.length; i++) {
                scene[i].represents.sprite = null;
            }
            scene.splice(0, scene.length);
        },
        loadImage: function(url, callback, pixelWidth, pixelHeight) {
            var img = new Image();
            if (typeof pixelWidth !== 'number' || isNaN(pixelWidth) || typeof pixelHeight !== 'number' || isNaN(pixelHeight)) {
                alert("pixel dimensions not given!");
            }
            img.pixelWidth = pixelWidth;
            img.pixelHeight = pixelHeight;
            $(img).bind('load', function() {
                callback(img);
            });
            img.src = url;
        },
        createFont: function(fontDef, image, number) {
            return new ags.WebGLRenderer.Font(this.engine, fontDef, image, number);
        },
        init: function() {
        },
        startFrames: function(fps) {
            var engine = this.engine;
            if (typeof fps === 'number') {
                engine.framesPerSecond = fps;
            }
            var existingInterval = this.frameInterval;
            if (existingInterval !== null) {
                window.clearInterval(existingInterval);
            }
            var renderer = this;
            this.frameInterval = window.setInterval(function(){
                $(engine).trigger('frame');
            }, 1000/engine.framesPerSecond);
        },
        stopFrames: function() {
            if (this.frameInterval !== null) {
                window.clearInterval(this.frameInterval);
                this.frameInterval = null;
            }
        },
        frameInterval: null,
        setFadeColor: function(r,g,b) {
            this.fadeRgb = 'rgba(' + r + ',' + g + ',' + b + ',';
        },
        fadeIn: function(r,g,b,frames) {
            this.setFadeColor(r,g,b);
            var alpha = 1;
            var perFrame = 1 / frames;
            var renderer = this;
            $(this.engine).bind('frame', function(e) {
                alpha = alpha - perFrame;
                if (alpha <= 0) {
                    renderer.fadeAlpha = 0;
                    $(this).unbind(e);
                    $(renderer).trigger('fadedIn');
                }
                else {
                    renderer.fadeAlpha = alpha;
                }
            });
        },
        fadeOut: function(r,g,b,frames) {
            this.fadeRgb = 'rgba(' + r + ',' + g + ',' + b + ',';
            var alpha = 0;
            var perFrame = 1 / frames;
            var renderer = this;
            $(this.engine).bind('frame', function(e) {
                alpha = alpha + perFrame;
                if (alpha >= 1) {
                    renderer.fadeAlpha = 1;
                    $(this).unbind(e);
                    $(renderer).trigger('fadedOut');
                }
                else {
                    renderer.fadeAlpha = alpha;
                }
            });
        },
        createTextOverlay: function(x,y,width,font,color,text) {
            var render = this.engine.game.fonts[font].renderString(text, color);
            var overlay = {
                type: 'text',
                x: x,
                y: y,
                width: width,
                font: font,
                color: color,
                text: text,
                render: render
            };
            this.overlays.push(overlay);
            return overlay;
        },
        setOverlayText: function(overlay,x,y,width,font,color,text) {
            var render = this.engine.game.fonts[font].renderString(text, color);
            overlay.type = 'text';
            overlay.x = x;
            overlay.y = y;
            overlay.width = width;
            overlay.font = font;
            overlay.color = color;
            overlay.text = text;
            overlay.render = render;
        },
        removeOverlay: function(overlay) {
            var i;
            for (i = 0; i < this.overlays.length; i++) {
                if (this.overlays[i] === overlay) {
                    this.overlays.splice(i, 1);
                    return;
                }
            }
        },
        createSprite: function() {
            return new ags.WebGLRenderer.Sprite(this);
        },
        createGUIDisplay: function(gui) {
            var display = new ags.WebGLRenderer.GUIDisplay(gui);
            this.guis.push(display);
            return display;
        },
        deleteSprite: function(spr) {
        },
        addSpriteToLayer: function(sprite, layerName) {
            switch(layerName) {
                case "scene":
                    this.scene.push(sprite);
                    this.reorderSprites();
                    break;
                case "overlays":
                    this.overlays.push(sprite);
                    break;
            }
        },
        removeSpriteFromLayer: function(sprite, layerName) {
            var layer;
            switch(layerName) {
                case "scene":
                    layer = this.scene;
                    break;
                case "overlays":
                    layer = this.overlays;
                    break;
                default:
                    return;
            }
            var i;
            for (i = 0; i < layer.length; i++) {
                if (layer[i] === sprite) {
                    layer.splice(i,1);
                    return;
                }
            }
        },
        startLoadingNumberedImage: function(imageNumber) {
            var engine = this.engine;
            var game = engine.game;
            var renderer = this;
            var def = game.def.numberedImages[imageNumber];
            if (imageNumber < this.numberedImages.length && this.numberedImages[imageNumber]) {
                return false;
            }
            if (typeof def.set === 'number') {
                if (this.numberedImageSets[def.set]) {
                    return false;
                }
                this.numberedImageSets[def.set] = "loading";
                var set = game.def.numberedImageSets[def.set];
                this.loadImage(game.getResourceLocation("numberedImageSet", {setNumber:def.set}),
                    function(setImg) {
                        var tempCanvas = document.createElement("CANVAS");
                        tempCanvas.width = set.w;
                        tempCanvas.height = set.h;
                        var tempCtx = tempCanvas.getContext('2d');
                        tempCtx.globalCompositeOperation = 'copy';
                        tempCtx.drawImage(setImg, 0, 0);
                        var i;
                        for (i = 0; i < game.def.numberedImages.length; i++) {
                            var otherDef = game.def.numberedImages[i];
                            if (otherDef.set === def.set) {
                                renderer.numberedImages[i] = {
                                    image: setImg,
                                    x: otherDef.x,
                                    y: otherDef.y,
                                    data: tempCtx.getImageData(otherDef.x, otherDef.y, otherDef.w, otherDef.h),
                                    w: otherDef.w,
                                    h: otherDef.h
                                };
                            }
                        }
                        $(renderer).trigger("loadedNumberedImage", [imageNumber]);
                    }, set.w, set.h);
                return true;
            }
            renderer.numberedImages[imageNumber] = "loading";
            this.loadImage(game.getResourceLocation("numberedImage", {imageNumber:imageNumber}),
                function(img) {
                    renderer.numberedImages[imageNumber] = {
                        image: img,
                        w: def.w,
                        h: def.h
                    };
                    $(renderer).trigger("loadedNumberedImage", [imageNumber]);
                });
            return true;
        },
        addNumberedImageToChecklist: function(checklist, imageNumber) {
            if (this.startLoadingNumberedImage(imageNumber)) {
                checklist.add("numberedImage");
                $(this).bind("loadedNumberedImage", function(e, loadedNumber) {
                    if (loadedNumber === imageNumber) {
                        checklist.checkOff("numberedImage");
                        $(this).unbind(e);
                    }
                });
                return true;
            }
            return false;
        },
        reorderSprites: function() {
            var sprites = this.scene;
            var i = 1, len = sprites.length;
            var temp;
            while (i < len) {
                var currentSprite = sprites[i], previousSprite = sprites[i-1];
                if ((currentSprite.baseline || currentSprite.y) < (previousSprite.baseline || previousSprite.y)) {
                    sprites[i] = previousSprite;
                    sprites[i-1] = currentSprite;
                    if (i > 1) i--;
                }
                else {
                    i++;
                }
            }
        }
    };
    ags.WebGLRenderer.Sprite = function(renderer) {
        this.renderer = renderer;
    };
    ags.WebGLRenderer.Sprite.prototype = {
        renderer: null,
        x: 0,
        multX: 0,
        y: 0,
        multY: 0,
        offX: 0,
        offY: 0,
        alpha: 1.0,
        image: -1,
        baseline: 0,
        visible: true,
        flipped: false,
        skipMoving: function() {
            $(this).trigger('skipMoving');
        },
        startMovingTowards: function(targetX, targetY, speed, linkMovementToAnimation) {
            var sprite = this;
            var renderer = this.renderer;
            var speedX = speed;
            var speedY = speed;
            var moveX = this.x;
            var moveY = this.y;
            var ang = Math.atan2(targetY - moveY, targetX - moveX);
            var stepX = speedX * Math.cos(ang);
            var stepY = speedY * Math.sin(ang);
            
            var updateFunc = function(e) {
                moveX += stepX;
                moveY += stepY;
                if ((stepX >= 0 && moveX >= targetX) || (stepX <= 0 && moveX <= targetX)) {
                    moveX = targetX;
                    stepX = 0;
                }
                if ((stepY >= 0 && moveY >= targetY) || (stepY <= 0 && moveY <= targetY)) {
                    moveY = targetY;
                    stepY = 0;
                }
                sprite.setParam("x", Math.floor(moveX));
                sprite.setParam("y", Math.floor(moveY));
                if (stepX === 0 && stepY === 0) {
                    $(sprite).unbind('skipMoving');
                    $(this).unbind(e);
                    $(sprite).trigger('arrived');
                }
            };
            
            var updateTarget, updateEvent;
            if (linkMovementToAnimation) {
                updateTarget = sprite;
                updateEvent = 'imageChanged';
            }
            else {
                updateTarget = renderer.engine;
                updateEvent = 'frame';
            }
            $(updateTarget).bind(updateEvent, updateFunc);
            $(sprite).bind('skipMoving', function(e) {
                $(updateTarget).unbind(updateEvent, updateFunc);
                sprite.setParam("x", targetX);
                sprite.setParam("y", targetY);
                $(sprite).trigger('arrived');
                $(this).unbind(e);
            });
        },
        setParam: function(paramName, paramValue) {
            this[paramName] = paramValue;
            switch(paramName) {
                case 'baseline':
                    this.renderer.reorderSprites();
                    break;
                case 'x':
                    break;
                case 'y':
                    if (this.baseline === 0) {
                        this.renderer.reorderSprites();
                    }
                    break;
                case 'image':
                    $(this).trigger('imageChanged');
                    break;
            }
        },
        setParams: function(params) {
            var paramName;
            for (paramName in params) {
                if (params.hasOwnProperty(paramName)) {
                    this.setParam(paramName, params[paramName]);
                }
            }
        },
        getParam: function(paramName) {
            return this[paramName];
        }
    };
    
    ags.WebGLRenderer.detectCompatibility = function() {
        return !!contextName;
    };
    
    ags.WebGLRenderer.Font = function(engine, fontDef, image, number) {
        ags.Font.call(this, engine, fontDef, number);
        var charsCanvas = document.createElement("CANVAS");
        charsCanvas.pixelWidth = image.pixelWidth;
        charsCanvas.pixelHeight = image.pixelHeight;
        charsCanvas.width = charsCanvas.pixelWidth;
        charsCanvas.height = charsCanvas.pixelHeight;
        var charsCtx = charsCanvas.getContext('2d');
        charsCtx.save();
        charsCtx.globalCompositeOperation = 'copy';
        charsCtx.drawImage(image, 0, 0);
        this.charsCanvas = charsCanvas;
        this.charsCtx = charsCtx;
        var outlineCanvas = document.createElement("CANVAS");
        outlineCanvas.pixelWidth = image.pixelWidth + 2;
        outlineCanvas.pixelHeight = image.pixelWidth + 2;
        outlineCanvas.width = outlineCanvas.pixelWidth;
        outlineCanvas.height = outlineCanvas.pixelHeight;
        var outlineCtx = outlineCanvas.getContext('2d');
        outlineCtx.drawImage(image, 0, 0);
        outlineCtx.drawImage(image, 1, 0);
        outlineCtx.drawImage(image, 2, 0);
        outlineCtx.drawImage(image, 0, 1);
        outlineCtx.drawImage(image, 2, 1);
        outlineCtx.drawImage(image, 0, 2);
        outlineCtx.drawImage(image, 1, 2);
        outlineCtx.drawImage(image, 2, 2);
        this.outlineCanvas = outlineCanvas;
        this.outlineCtx = outlineCtx;
    };
    ags.WebGLRenderer.Font.prototype = $.extend({}, ags.Font.prototype, {
        renderString: function(str, textColor) {
            var charsCanvas = this.charsCanvas;
            var charsCtx = this.charsCtx;
            var outlineCanvas = this.outlineCanvas;
            var outlineCtx = this.outlineCtx;
            
            textColor = this.engine.getColorRgbString(textColor);
            var textShadowColor = this.engine.getColorRgbString(this.engine.game.game.text_shadow_color);
            
            outlineCtx.save();
            outlineCtx.globalCompositeOperation = 'source-in';
            outlineCtx.fillStyle = textShadowColor;
            outlineCtx.fillRect(0, 0, outlineCanvas.pixelWidth, outlineCanvas.pixelHeight);
            outlineCtx.restore();
            
            charsCtx.save();
            charsCtx.globalCompositeOperation = 'source-in';
            charsCtx.fillStyle = textColor;
            charsCtx.fillRect(0, 0, charsCanvas.pixelWidth, charsCanvas.pixelHeight);
            charsCtx.restore();
            
            var renderCanvas = document.createElement("CANVAS");
            renderCanvas.width = this.stringWidth(str) + 2;
            renderCanvas.height = this.def.lineHeight + 2;
            var renderCtx = renderCanvas.getContext('2d');
            
            var def = this.def;
            var chars = def.chars;
            var i, x, y;
            for (i = 0, x = 0, y = 0; i < str.length; i++) {
                var c = str.charCodeAt(i);
                if (c < chars.length && chars[c]) {
                    c = chars[c];
                    if (c.w > 0 && c.h > 0) {
                        renderCtx.drawImage(outlineCanvas, c.x, c.y, c.w + 2, c.h + 2, x + c.xo, y + c.yo, c.w + 2, c.h + 2);
                    }
                    x += c.a;
                }
            }
            
            for (i = 0, x = 1, y = 1; i < str.length; i++) {
                var c = str.charCodeAt(i);
                if (c < chars.length && chars[c]) {
                    c = chars[c];
                    if (c.w > 0 && c.h > 0) {
                        renderCtx.drawImage(charsCanvas, c.x, c.y, c.w, c.h, x + c.xo, y + c.yo, c.w, c.h);
                    }
                    x += c.a;
                }
            }
            
            return renderCanvas;
        }
    });
    
    ags.WebGLRenderer.GUIDisplay = function(gui) {
        this.gui = gui;
    };
    ags.WebGLRenderer.GUIDisplay.prototype = {
    };
    
    /*
     *
     * SCALING
     *
     */
     
    // Shader Source Code
    
    var simpleShader = [
        "#ifdef GL_ES",
        "precision highp float;",
        "#endif",
        
        "varying vec2 vTextureCoord;",
        
        "uniform sampler2D uSampler;",
        
        "void main(void) {",
            "gl_FragColor = texture2D(uSampler, vec2(vTextureCoord.s, vTextureCoord.t));",
        "}",
        ""
    ].join("\r\n");
    
    var scale2xShader = [
        "#ifdef GL_ES",
        "precision highp float;",
        "#endif",

        "varying vec2 vTextureCoord;",

        "uniform sampler2D uSampler;",
        "uniform float pixelWidth;",
        "uniform float pixelHeight;",

        "void main(void) {",
            
            "int x = int(vTextureCoord.x * pixelWidth);",
            "int xi = x - 2 * (x/2);",
            "x /= 2;",
            "int y = int(vTextureCoord.y * pixelHeight);",
            "int yi = y - 2 * (y/2);",
            "y /= 2;",
            "vec2 coord2 = vec2(float(x), float(y));",
            "vec2 divBy = vec2(pixelWidth, pixelHeight);",
            "vec4 B = texture2D(uSampler, (coord2 + vec2( 0.0, -1.0)) / divBy);",
            "vec4 D = texture2D(uSampler, (coord2 + vec2(-1.0, 0.0)) / divBy);",
            "vec4 E = texture2D(uSampler, coord2 / divBy);",
            "vec4 F = texture2D(uSampler, (coord2 + vec2(1.0, 0.0)) / divBy);",
            "vec4 H = texture2D(uSampler, (coord2 + vec2( 0.0, 1.0)) / divBy);",
            "if (B == H || D == F) {",
                "gl_FragColor = E;",
            "}",
            "else {",
                "if (yi == 0) {",
                    "if (xi == 0) {",
                        "if (D == B) { gl_FragColor = D; } else { gl_FragColor = E; }",
                    "}",
                    "else {",
                        "if (B == F) { gl_FragColor = F; } else { gl_FragColor = E; }",
                    "}",
                "}",
                "else {",
                    "if (xi == 0) {",
                        "if (D == H) { gl_FragColor = D; } else { gl_FragColor = E; }",
                    "}",
                    "else {",
                        "if (H == F) { gl_FragColor = F; } else { gl_FragColor = E; }",
                    "}",
                "}",
            "}",
        "}",
        ""
    ].join('\r\n');
    
    var scale3xShader = [
        "#ifdef GL_ES",
        "precision highp float;",
        "#endif",

        "varying vec2 vTextureCoord;",

        "uniform sampler2D uSampler;",
        "uniform float pixelWidth;",
        "uniform float pixelHeight;",

        "void main(void) {",
            
            "int x = int(vTextureCoord.x * pixelWidth);",
            "int xi = x - 3 * (x/3);",
            "x /= 3;",
            "int y = int(vTextureCoord.y * pixelHeight);",
            "int yi = y - 3 * (y/3);",
            "y /= 3;",
            "vec2 coord2 = vec2(float(x), float(y));",
            "vec2 divBy = vec2(pixelWidth, pixelHeight);",
            "vec4 A = texture2D(uSampler, (coord2 + vec2(-1.0, -1.0)) / divBy);",
            "vec4 B = texture2D(uSampler, (coord2 + vec2( 0.0, -1.0)) / divBy);",
            "vec4 C = texture2D(uSampler, (coord2 + vec2( 1.0, -1.0)) / divBy);",
            "vec4 D = texture2D(uSampler, (coord2 + vec2(-1.0, 0.0)) / divBy);",
            "vec4 E = texture2D(uSampler, coord2 / divBy);",
            "vec4 F = texture2D(uSampler, (coord2 + vec2(1.0, 0.0)) / divBy);",
            "vec4 G = texture2D(uSampler, (coord2 + vec2(-1.0, 1.0)) / divBy);",
            "vec4 H = texture2D(uSampler, (coord2 + vec2( 0.0, 1.0)) / divBy);",
            "vec4 I = texture2D(uSampler, (coord2 + vec2( 1.0, 1.0)) / divBy);",
            "if (B == H || D == F) {",
                "gl_FragColor = E;",
            "}",
            "else {",
                "if (yi == 0) {",
                    "if (xi == 0) {",
                        "if (D == B) { gl_FragColor = D; } else { gl_FragColor = E; }",
                    "}",
                    "else if (x == 1) {",
                        "if ((D == B && E != C) || (B == F && E != A)) { gl_FragColor = B; } else { gl_FragColor = A; }",
                    "}",
                    "else {",
                        "if (B == F) { gl_FragColor = F; } else { gl_FragColor = E; }",
                    "}",
                "}",
                "else if (yi == 1) {",
                    "if (xi == 0) {",
                        "if ((D == B && E != G) || (D == H && E != A)) { gl_FragColor = D; } else { gl_FragColor = E; }",
                    "}",
                    "else if (x == 1) {",
                        "gl_FragColor = E;",
                    "}",
                    "else {",
                        "if ((B == F && E != I) || (H == F && E != C)) { gl_FragColor = F; } else { gl_FragColor = E; }",
                    "}",
                "}",
                "else {",
                    "if (xi == 0) {",
                        "if (D == H) { gl_FragColor = D; } else { gl_FragColor = E; }",
                    "}",
                    "else if (x == 1) {",
                        "if ((D == H && E != I) || (H == F && E != G)) { gl_FragColor = H; } else { gl_FragColor = E; }",
                    "}",
                    "else {",
                        "if (H == F) { gl_FragColor = F; } else { gl_FragColor = E; }",
                    "}",
                "}",
            "}",
        "}",
        ""
    ].join("\r\n");
    
    // Set up Scaler objects
    
    var WebGLScaler = function(def) {
        this.scale = def.scale;
        this.category = def.category;
        this.times = (typeof def.times === 'number') ? def.times : 1;
    };
    WebGLScaler.prototype = {
        applyTo: function(webglCanvas) {
            
        }
    };

    ags.WebGLRenderer.scalers = [];
    
    // Define scalers
    
    ags.WebGLRenderer.scalers.push(new WebGLScaler({
        category: "Pixelly",
        scale: 2,
        fragmentShader: simpleShader
    }));
    
    ags.WebGLRenderer.scalers.push(new WebGLScaler({
        category: "Pixelly",
        scale: 3,
        fragmentShader: simpleShader
    });
    
    ags.WebGLRenderer.scalers.push(new WebGLScaler({
        category: "Pixelly",
        scale: 4,
        fragmentShader: simpleShader
    });
    
    ags.WebGLRenderer.scalers.push(new WebGLScaler({
        category: "Scale-x",
        scale: 2,
        fragmentShader: scale2xShader
    });
    
    ags.WebGLRenderer.scalers.push(new WebGLScaler({
        category: "Scale-x",
        scale: 3,
        fragmentShader: scale3xShader
    });
    
    ags.WebGLRenderer.scalers.push(new WebGLScaler({
        category: "Scale-x",
        scale: 4,
        fragmentShader: scale2xShader,
        times: 2
    });
    
});
