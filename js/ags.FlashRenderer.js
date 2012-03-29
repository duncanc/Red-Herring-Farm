
jQuery(function($) {

    ags.FlashRenderer = function(engine, swfUrl, replaceElemId) {
        this.engine = engine;
        var renderer = this;
        var prefix = "_" + ags.util.randomString();
        var mouse = engine.mouse;
        
        var sprites = {};
        this.sprites = sprites;
        
        window[prefix + "_rendererLoaded"] = function() {
            renderer.isSwfLoaded = true;
            renderer.swf.setGameData({
                numberedImages:engine.game.def.numberedImages,
                resourceLocations:engine.game.def.resourceLocations
            });
            $(renderer).trigger('loaded');
        };
        window[prefix + "_fadedIn"] = function() {
            $(renderer).trigger('fadedIn');
        };
        window[prefix + "_loadedNumberedImage"] = function(imageNumber) {
            $(renderer).trigger('loadedNumberedImage', [imageNumber]);
        };
        window[prefix + "_fadedOut"] = function() {
            $(renderer).trigger('fadedOut');
        };
        window[prefix + "_keyDown"] = function(keycode) {
            $(engine).trigger('keyDown', [keycode]);
        };
        window[prefix + "_keyUp"] = function(keycode) {
            $(engine).trigger('keyUp', [keycode]);
        };
        window[prefix + "_mouseDown"] = function(button) {
            $(engine).trigger('mouseDown', [button]);
        };
        window[prefix + "_mouseUp"] = function(button) {
            $(engine).trigger('mouseUp', [button]);
        };
        window[prefix + "_frame"] = function() {
            $(engine).trigger('frame');
        };
        window[prefix + "_mouseMoved"] = function(x, y) {
            mouse.nextOnScreen = true;
            mouse.nextX = x;
            mouse.nextY = y;
        };
        window[prefix + "_mouseLeft"] = function() {
            $(engine).trigger("mouseLeft");
            mouse.nextOnScreen = false;
        };
        window[prefix + "_imageLoaded"] = function(imgName) {
            $(renderer).trigger('imageLoaded', [imgName]);
        };
        window[prefix + "_spriteArrived"] = function(spriteName) {
            var sprite = sprites[spriteName];
            $(sprite).trigger('arrived');
        };
        window[prefix + "_clearSprite"] = function(spriteName) {
            var sprite = sprites[spriteName];
            sprite.represents.sprite = null;
            delete sprites[spriteName];
        };
        window[prefix + "_mouseOverSprite"] = function(spriteName) {
            if (!spriteName) {
                $(engine).trigger('mouseMovedOver', [null]);
            }
            else {
                var sprite = sprites[spriteName];
                $(engine).trigger('mouseMovedOver', [sprite && sprite.represents || null]);
            }
        };
        window[prefix + "_mouseOverHotspot"] = function(hotspotNumber) {
            if (hotspotNumber === 0) {
                $(engine).trigger('mouseMovedOver', [null]);
            }
            else {
                $(engine).trigger('mouseMovedOver', [engine.game.hotspot[hotspotNumber]]);
            }
        };
        $(engine).bind('setInteractive', function(e, interactive) {
            renderer.swf.setInteractive(interactive);
        });
        
        swfobject.embedSWF(
            /* swfUrl */ swfUrl + "?" + Math.random(),
            /* replaceElemId */ replaceElemId,
            /* width */ engine.game.def.settings.width.toString(),
            /* height */ engine.game.def.settings.height.toString(),
            /* swfVersion */ "10.0.0",
            /* xiSwfUrl */ null, // "swfobject/expressInstall.swf",
            /* flashVars */ {prefix:prefix},
            /* params */ {allowScriptAccess:"always", allowFullScreen:"true", bgcolor:"#000000"},
            /* attributes */ {},
            function/* callbackFn */(e){
                if (e.success) {
                    renderer.swf = e.ref;
                    renderer.mouseCursorSprite = new ags.FlashRenderer.Sprite(e.ref, "mouseCursor");
                }
            });
        
    };
    ags.FlashRenderer.prototype = {
        swf: null,
        isSwfLoaded: false,
        mouseCursorSprite: null,
        getMouseCursorSprite: function() {
            return this.mouseCursorSprite;
        },
        createGUIDisplay: function(gui) {
            var renderer = this;
            $(gui).bind('changedParam', function(e, paramName, paramValue) {
                renderer.swf.setGUIParam(gui.number, paramName, paramValue);
            });
            this.swf.createGUIDisplay(gui.number, gui.def, gui.visible);
            for (var i = 0; i < gui.controls.length; i++) {
                (function(control){
                
                    switch(control.def.type) {
                        case 'label':
                            $(control).bind('changedParam', function(e, paramName, paramValue) {
                                renderer.swf.setLabelParam(gui.number, control.number, paramName, paramValue);
                            });
                            break;
                    }
                
                })(gui.controls[i]);
            }
        },
        clearScene: function() {
            this.swf.clearScene();
        },
        setWalkbehinds: function(walkbehinds) {
            this.swf.setWalkbehinds(walkbehinds);
        },
        setWalkbehindBase: function(wbNumber, baseline) {
            this.swf.setWalkbehindBase(wbNumber, baseline);
        },
        loadImage: function(url, callback, pixelWidth, pixelHeight) {
            $(this).bind('imageLoaded', function(e, loadedUrl) {
                if (loadedUrl == url) {
                    callback(loadedUrl);
                    $(this).unbind(e);
                }
            });
            this.swf.loadImage(url);
        },
        setHotspotMask: function(mask) {
            this.swf.setHotspotMask(mask);
        },
        setBackground: function(bg) {
            this.swf.setBackground(bg);
        },
        init: function(list) {
            if (!this.isSwfLoaded) {
                list.add("flashRenderer");
                $(this).bind('loaded', function() {
                    list.checkOff("flashRenderer");
                });
            }
        },
        createFont: function(fontDef, img, num) {
            return new ags.FlashRenderer.Font(this.swf, fontDef, img, num);
        },
        startFrames: function(fps) {
            var engine = this.engine;
            if (typeof fps === 'number') {
                engine.framesPerSecond = fps;
            }
            this.swf.startFrames(engine.framesPerSecond);
        },
        stopFrames: function() {
            this.swf.stopFrames();
        },
        setFadeColor: function(r,g,b) {
            this.swf.setFadeColor(r,g,b);
        },
        fadeIn: function(r,g,b,frames) {
            this.swf.fadeIn(r,g,b,frames);
        },
        fadeOut: function(r,g,b,frames) {
            this.swf.fadeOut(r,g,b,frames);
        },
        createTextOverlay: function(x,y,width,font,color,text) {
            return {
                x: x,
                y: y,
                width: width,
                font: font,
                color: color,
                text: text,
                name: this.swf.createTextOverlay(
                    x,
                    y,
                    width,
                    font,
                    this.engine.getColor(color),
                    this.engine.getColor(this.engine.game.game.text_shadow_color),
                    text)
            };
        },
        setOverlayText: function(overlay,x,y,width,font,color,text) {
            overlay.x = x;
            overlay.y = y;
            overlay.width = width;
            overlay.font = font;
            overlay.color = color;
            overlay.text = text;
            this.swf.setTextOverlay(
                overlay.name,
                x,
                y,
                width,
                font,
                this.engine.getColor(color),
                this.engine.getColor(this.engine.game.game.text_shadow_color),
                text);
        },
        removeOverlay: function(overlay) {
            this.swf.removeOverlay(overlay.name);
        },
        createSprite: function() {
            var spriteName = "_" + ags.util.randomString();
            this.swf.createSprite(spriteName);
            var sprite = new ags.FlashRenderer.Sprite(this.swf, spriteName);
            return sprite;
        },
        addSpriteToLayer: function(sprite, layerName) {
            this.swf.addSpriteToLayer(sprite.name, layerName);
            this.sprites[sprite.name] = sprite;
        },
        redrawWalkbehinds: function() {
        },
        removeSpriteFromLayer: function(sprite, layerName) {
            delete this.sprites[sprite.name];
            this.swf.removeSpriteFromLayer(sprite.name, layerName);
        },
        addNumberedImageToChecklist: function(checklist, imageNumber) {
            if (this.swf.startLoadingNumberedImage(imageNumber)) {
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
        }
    };
    
    ags.FlashRenderer.Sprite = function(swf, name) {
        this.swf = swf;
        this.name = name;
    };
    
    ags.FlashRenderer.Sprite.prototype = {
        swf: null,
        name: null,
        setParam: function(paramName, paramValue) {
            this.swf.setSpriteParam(this.name, paramName, paramValue);
        },
        setParams: function(params) {
            this.swf.setSpriteParams(this.name, params);
        },
        getParam: function(paramName) {
            return this.swf.getSpriteParam(this.name, paramName);
        },
        startMovingTowards: function(targetX, targetY, speed, linkMovementToAnimation) {
            this.swf.startSpriteMovingTowards(this.name, targetX, targetY, speed, linkMovementToAnimation);
        },
        skipMoving: function() {
            this.swf.skipSpriteMoving(this.name);
        }
    };
    
    ags.FlashRenderer.Overlay = function(swf,x,y,width,font,color,text) {
        this.swf = swf;
        this.name = swf.createTextOverlay(x,y,width,font,color,text);
        this.x = x;
        this.y = y;
        this.width = width;
        this.font = font;
        this.color = color;
        this.font = font;
    }
    ags.FlashRenderer.Overlay.prototype = {
    };
    
    ags.FlashRenderer.Font = function(swf, fontDef, img, num) {
        swf.createFont(num, fontDef, img);
        this.def = fontDef;
        this.number = num;
        this.swf = swf;
    };
    
    ags.FlashRenderer.Font.prototype = $.extend(ags.Font.prototype, {
    });
    
    ags.FlashRenderer.detectCompatibility = function() {
        return ags.util.detectFlash();
    };
    
});
