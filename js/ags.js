
ags = {
    games: {}
};

jQuery(function($) {

    ags.util = {
        lpad: function(str, chr, count) {
            while (str.length < count) str = chr + str;
            return str;
        },
        rpad: function(str, chr, count) {
            while (str.length < count) str = str + chr;
            return str;
        },
        extractVarargs: function(num, array) {
            array.push(array.splice(array.length - num, num));
            return array;
        },
        fmt_f: function(number, precision) {
            if (precision === 0) {
                return Math.round(num).toString();
            }
            var pp = Math.pow(10, precision);
            var numString = (Math.round(num * pp) / pp).toString();
            var ptPos = numString.indexOf(".");
            if (ptPos === -1) {
                ptPos = numString.length;
                numString += ".";
            }
            return ags.util.rpad(numString, "0", ptPos + 1 + precision);
        },
        format: function(str, fmtArgs) {
            if (!fmtArgs) return str;
            var i = 0;
            return str.replace(/%\.?([0-9]*)([dscf%])/gi, function(all, digits, type) {
				switch(type.toLowerCase()) {
					case "d":
						if (digits.length > 0) {
							return ags.util.lpad(fmtArgs[i++].toString(), "0", parseInt(digits));
						}
						return fmtArgs[i++];
					case "f":
						return ags.util.fmt_f(
							parseFloat(fmtArgs[i++]),
							(digits.length > 0) ? parseInt(digits) : 6);
					case "c": return String.fromCharCode(fmtArgs[i++]);
					case "s": return fmtArgs[i++];
					case "%": return "%";
					default: return all;
				}
			});
        },
        regexEscape: function(str) {
            return str.replace(/([\[\]\\\*\+\.\?\(\)\^\$\{\}])/g, '\\$1');
        },
        randomString: function() {
            var str = '';
            var firstRandomChars = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var randomChars = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            str = firstRandomChars.charAt(Math.floor(Math.random() * firstRandomChars.length));
            for (var i = 0; i < 10; i++) {
                str += randomChars.charAt(Math.floor(Math.random() * randomChars.length));
            }
            return str;
        },
        okToPreventKeyEvent: function(keyEvent) {
            if (keyEvent.keyCode === 116 /* F5 */ || keyEvent.keyCode === 122 /* F11 */) {
                return false;
            }
            if (keyEvent.ctrlKey) {
                switch(keyEvent.keyCode) {
                    case 9: /* TAB */
                    case 84: /* T */
                    case 76: /* L */
                    case 48: /* 0 */
                    case 107: /* =/+ */
                    case 109: /* - */
                        return false;
                }
            }
            if (keyEvent.altKey) {
                if (keyEvent.keyCode === 68 /* 'D' */) {
                    return false;
                }
            }
            return true;
        },
        blockingFunction: function(paramNames, func) {
            func.blocking = true;
            func.paramNames = paramNames;
            return func;
        },
        structArray: function(count, struct) {
            var array = new Array(count);
            for (var i = 0; i < count; i++) {
                array[i] = new struct();
            }
            return array;
        },
        fillArray: function(count, value) {
            var array = new Array(count);
            for (var i = 0; i < count; i++) {
                array[i] = value;
            }
            return array;
        },
        createStruct: function(ctor, proto) {
            ctor.prototype = proto;
            return ctor;
        },
        // you can flawlessly simulate int32 addition, subtraction and division
        // by casting the result with double-precision floating-point numbers
        // (that are both known to represent valid int32 values) to an int32.
        // not so with multiplication. so this is a special function for
        // multiplying two integers together using bitwise operations.
        imul: function(a, b) {
            var c = 0;
            while (b !== 0) {
                if (b&1 !== 0) {
                    c = (c + a) | 0;
                }
                a <<= 1;
                b >>>= 1;
            }
            return c;
        },
        getKeyCode: function(keyEvent) {
			if (keyEvent.ctrlKey && keyEvent.keyCode !== 17 && keyEvent.keyCode !== 18) {
				// Ctrl+A to Ctrl+Z
				if (keyEvent.keyCode >= 65 && keyEvent.keyCode <= 90) {
					return keyEvent.keyCode + (1 - 65);
				}
				// anything else do not count as a keycode
				return -1;
			}
			// F1 to F10 (F11 and F12 are separate)
			if (keyEvent.keyCode >= 112 && keyEvent.keyCode <= 121) {
				return keyEvent.keyCode + (359 - 112);
			}
			if (keyEvent.keyCode >= 65 && keyEvent.keyCode <= 90) {
				return keyEvent.keyCode; // use keyCode not charCode of letters
			}
			switch(keyEvent.keyCode) {
				case 122: // F11
					return 433;
				case 123: // F12
					return 434;
				case 46: // delete
					return 383;
				case 45: // insert
					return 382;
				
				case 36: // home
					return 371;
				case 38: // up arrow
					return 372;
				case 33: // page up
					return 373;
				case 37: // left arrow
					return 375;
				case 101: // numpad 5
					return 376;
				case 39: // right arrow
					return 377;
				case 35: // end
					return 379;
				case 40: // down arrow
					return 380;
				case 34: // page down
					return 381;
					
				case 16: // shift
					return 403; // left shift; 404 is right shift but we cannot differentiate
				case 17: // ctrl
					return 405; // left ctrl, same situation
				case 18: // alt
					return 407;
			}
			return keyEvent.keyCode || keyEvent.charCode;
        },
        detectFlash: function() {
            // Firefox/Chrome/Opera/Safari
            if (typeof window.navigator !== 'undefined' && typeof navigator.plugins !== 'undefined') {
                for (var i = 0; i < navigator.plugins.length; i++) {
                    if (/^shockwave flash/i.test(navigator.plugins[i].name)) {
                        return true;
                    }
                }
            }
            // IE
            if (typeof window.ActiveXObject !== 'undefined') {
                try {
                    var _ = new ActiveXObject("ShockwaveFlash.ShockwaveFlash");
                    return true;
                }
                catch (e) {
                    return false;
                }
            }
            // Assume not
            return false;
        }
    };

    /*
     helper class for tracking that everything necessary has been done for something to happen
    
     for example:
       var cakeChecklist = new ags.Checklist("cake");
       $(cakeChecklist).bind("allCheckedOff", function(){ alert("OK! Time to make that cake!"); });
       cakeChecklist.add("egg", 2);
       cakeChecklist.add("flour");
       cakeChecklist.done(); // IMPORTANT!!
    
       ...
     
       cakeChecklist.checkOff("egg");
       cakeChecklist.checkOff("flour");
       cakeChecklist.checkOff("egg"); // OK! Time to make that cake!
    */
    ags.Checklist = function(name) {
        if (typeof name === 'string') {
            this.name = name;
        }
    };
    ags.Checklist.prototype = {
        name: "untitled",
        complete: false,
        debugMessage: function() {
            var list = this.name + " checklist still waiting on:";
            for (var item in this) {
                var itemName = item.match(/^item\$(.*)$/);
                if (itemName && this[item] !== 0) {
                    list += "\r\n" + this[item] + "x " + itemName[1];
                }
            }
            alert(list);
        },
        checkForCompleteness: function() {
            for (var item in this) {
                if (/^item\$/.test(item) && this[item] !== 0) {
                    return false;
                }
            }
            $(this).trigger('allCheckedOff');
            $(this).unbind('allCheckedOff');
            return true;
        },
        done: function() {
            this.complete = true;
            if (!this.checkForCompleteness()) {
                // debug: warn after 5 seconds
                var list = this;
                list.interval = window.setTimeout(function() {
                    list.debugMessage();
                }, 5000);
                $(list).bind('allCheckedOff', function(){
                    window.clearTimeout(list.interval);
                    list.interval = null;
                });
                // end debug
            }
        },
        add: function(item, count) {
            if (typeof count !== 'number') {
                count = 1;
            }
            var itemKey = "item$" + item;
            var t = typeof this[itemKey];
            if (t === 'undefined') {
                this[itemKey] = count;
            }
            else if (t === 'number') {
                this[itemKey] += count;
            }
            else {
                alert(this.name + " checklist: invalid item name \"" + item + "\"");
            }
        },
        checkOff: function(item, count) {
            if (typeof count !== 'number') {
                count = 1;
            }
            var itemKey = "item$" + item;
            if (typeof this[itemKey] !== 'number' || this[itemKey] < count) {
                alert(this.name + ' checklist: unbalanced number of ' + item);
                return;
            }
            this[itemKey] -= count;
            if (this.complete && this[itemKey] === 0) {
                this.checkForCompleteness();
            }
        }
    };
    
    var util = ags.util;

    ags.Engine = function() {
        this.mouse = {x:0, y:0, nextX:0, nextY:0, onScreen:false, nextOnScreen:false};
        this.fileSystem = new ags.NullFileSystem();
        
        var engine = this;
        this.keyDownHandler = function(e) {
            var keycode = ags.util.getKeyCode(e);
            if (keycode !== -1) {
                $(engine).trigger('keyDown', [keycode]);
                if (ags.util.okToPreventKeyEvent(e)) {
                    e.preventDefault();
                    return false;
                }
            }
        };
        this.keyUpHandler = function(e) {
            var keycode = ags.util.getKeyCode(e);
            if (keycode !== -1) {
                $(engine).trigger('keyUp', [keycode]);
                if (ags.util.okToPreventKeyEvent(e)) {
                    e.preventDefault();
                    return false;
                }
            }
        };
        $(this).bind("loadedGlobalScripts", function(e) {
            $(this).unbind(e);
        });
        this.blockingThread = new ags.ScriptContext(this);
        this.nonBlockingThread = new ags.ScriptContext(this);
        this.roomThread = new ags.ScriptContext(this);
        $(this).bind("frame", function() {
            var blockingThread = engine.blockingThread;
            blockingThread.run();
            var roomThread = engine.roomThread;
            roomThread.run();
            var newInteractive = !blockingThread.isBlocked() && !roomThread.isBlocked();
            if (engine.interactive !== newInteractive) {
                $(engine).trigger('setInteractive', [newInteractive]);
                engine.interactive = newInteractive;
            }
        });
        this.audioPlayers = [];
        
        $(this).bind('mouseLeft', function(e, mouseOver) {
            $(engine).trigger('mouseMovedOver', [null]);
        });
        
        $(this).bind('mouseMovedOver', function(e, mouseOver) {
            engine.mouseOver = mouseOver;
        });
        
        $(this).bind('mouseDown', function(e, mouseButton) {
            if (engine.interactive) {
                engine.blockingThread.beginCall(engine.game.globalScripts.onMouseClick, [mouseButton]);
            }
        });
        
    };
    ags.Engine.prototype = {
        game: null,
        interactive: false,
        
        audioPlayers: null,
        addAudioPlayer: function(audioPlayer) {
            this.audioPlayers.push(audioPlayer);
        },
        
        blockingThread: null,
        nonBlockingThread: null,
        roomThread: null,
        
        load: function(gameDef, baseURL) {
            var game = new ags.Game(this, gameDef);
						game.baseURL = baseURL || "";
            this.game = game;
            var games = ags.games;
            var engine = this;
            var guid = gameDef.guid;
            if (typeof games[guid] === 'undefined') {
                games[guid] = {};
            }
            if (typeof games[guid].GlobalScripts !== 'undefined') {
                game.globalScripts = new games[guid].GlobalScripts(engine);
                $(engine).trigger("loadedGlobalScripts");
            }
            else {
                $.ajax({
                    url: game.getResourceLocation("globalScripts"),
                    dataType: 'script',
                    success: function() {
                        game.globalScripts = new games[guid].GlobalScripts(engine);
                        $(engine).trigger("loadedGlobalScripts");
                    }
                });
            }
            return game;
        },
        
        keyDownHandler: null,
        keyUpHandler: null,
        
        play: function() {
            var engine = this;
            var renderer = this.renderer;
            var game = this.game;
            
            var startList = new ags.Checklist('renderer');
            
            $(startList).bind('allCheckedOff', function() {
                
                var gameList = new ags.Checklist('game');
                
                $(gameList).bind('allCheckedOff', function() {
                    $(engine).trigger('begin');
                    
                    // TODO: do properly
                    var mouseCursorSprite = engine.renderer.getMouseCursorSprite();
                    mouseCursorSprite.setParams({
                        image: 14,
                        offX: -1,
                        offY: -1
                    });
                    
                    for (var i = 0; i < game.gui.length; i++) {
                        var gui = game.gui[i];
                        gui.display = renderer.createGUIDisplay(gui);
                    }
                    
                    game.globalScripts.gameStart();
                    engine.loadRoom(game.player.room);
                });
                
                // TODO: do properly
                
                game.init(gameList);
                engine.renderer.addNumberedImageToChecklist(gameList, 14);
                
                gameList.done();
                
            });
            
            renderer.init(startList);
            
            startList.done();
        },
        
        startPlaying: function() {
            
            if (typeof this.game.globalScripts !== 'undefined') {
                this.reallyStartPlaying();
            }
            else {
                var engine = this;
                $(this).bind('loadedGlobalScripts', function(e) {
                    engine.reallyStartPlaying();
                    $(this).unbind(e);
                });
            }
            
        },
        reallyStartPlaying: function() {
            var engine = this;
            var renderer = this.renderer;
            var game = this.game;
            game.globalScripts.gameStart();
            engine.loadRoom(game.player.room);
        },
        
        loadRoom: function(roomNumber, whenLoaded) {
            var engine = this;
            var game = this.game;
            var ajaxParams = {
                url: game.getResourceLocation("roomData", {roomNumber:roomNumber}),
                cache: false,
                type: "GET",
                dataType: "json",
                success: function(roomDef) {
                
                    var room = new ags.Room(engine, roomDef, roomNumber);
                    engine.room = room;
                
                }
            };
            if (this.room) {
                this.blockingThread.queueCall(this.FadeOut, [3]);
                this.blockingThread.queueCall(function() {
                    $.ajax(ajaxParams);
                });
            }
            else {
                $.ajax(ajaxParams);
            }
        },
        
        // replace
        sniffOperatingSystem: function() {
            var appVersion = navigator && navigator.appVersion;
            if (appVersion) {
                if (appVersion.indexOf("Linux") !== -1) return "linux";
                if (appVersion.indexOf("Mac") !== -1) return "macos";
                if (appVersion.indexOf("Win") !== -1) return "windows";
            }
            return null;
        },
        versionString: "Web AGS v0.01 alpha",
        fileSystem: null,
        mouse: null,
        
        // utilities
        numberedFiles: null,
        addNumberedFile: function(f) {
            var numberedFiles = this.numberedFiles;
            if (numberedFiles === null) {
                numberedFiles = [null];
                this.numberedFiles = numberedFiles;
            }
            numberedFiles.push(f);
            return numberedFiles.length - 1;
        },
        getNumberedFile: function(n) {
            return this.numberedFiles[n];
        },
        removeNumberedFile: function(n) {
            this.numberedFiles[n] = null;
        },
        framesPerSecond: 40,
        
        /*** Scripting API: Useful Utilities ***/
        
        // Numerical Utilities
        Maths$$ArcCos:           Math.acos,
        Maths$$ArcSin:           Math.asin,
        Maths$$ArcTan:           Math.atan,
        Maths$$ArcTan2:          Math.atan2,
        Maths$$Cos:              Math.cos,
        Maths$$RaiseToPower:     Math.pow,
        Maths$$Sin:              Math.sin,
        Maths$$Sqrt:             Math.sqrt,
        Maths$$Tan:              Math.tan,
        Maths$$Exp:              Math.exp,
        Maths$$Log:              Math.log,
        Maths$$Log10:            function(x)   { return Math.log(x) / Math.log(10);     },
        Maths$$DegreesToRadians: function(deg) { return deg * (Math.PI / 180);          },
        Maths$$RadiansToDegrees: function(rad) { return rad * (180 / Math.PI);          },
        Maths$$get_Pi:           function()    { return Math.PI;                        },
        Random:                  function(n)   { return (Math.random() * (n+1)) | 0;    },
        IntToFloat:              function(x)   { return x;                              },
        FloatToInt: function(x, dir) {
            switch(dir) {
                case 0 /* eRoundDown */: return Math.floor(x);
                case 1: return Math.round(x);
                case 2 /* eRoundUp */: return Math.ceil(x);
                default: return Math.round(x);
            }
        },
        Maths$$Sinh: function(x) { return (Math.exp(x) - Math.exp(-x))/2;          },
        Maths$$Cosh: function(x) { return (Math.exp(x) + Math.exp(-x))/2;          },
        Maths$$Tanh: function(x) { return this.Math$$Sinh(x) / this.Math$$Cosh(x); },
        
        // String Utilities
        String$$AsFloat:       parseFloat,
        String$$AsInt:         parseInt,
        String$$IsNullOrEmpty: function(stringToCheck)    { return (!stringToCheck) | 0;                    },
        String$$Append:        function(str, append)      { return str + append;                            },
        String$$AppendChar:    function(str, ch)          { return str + String.fromCharCode(ch);           },
        String$$Format:        function(str, fmtArgs)     { return ags.format(str, fmtArgs);                },
        String$$UpperCase:     function(str)              { return str.toUpperCase();                       },
        String$$geti_Chars:    function(str, i)           { return str.charCodeAt(i);                       },
        StrGetCharAt:          function(str, i)           { return str.charCodeAt(i);                       },
        String$$get_Length:    function(str)              { return str.length;                              },
        String$$Copy:          function(str)              { return str;                                     },
        String$$Contains:      function(haystack, needle) { return (haystack.indexOf(needle) !== -1) | 0;   },
        String$$IndexOf:       function(haystack, needle) { return haystack.indexOf(needle);                },
        String$$LowerCase:     function(str)              { return str.toLowerCase();                       },
        String$$Substring:     function(str, i, len)      { return str.slice(i, i+len);                     },
        String$$Truncate:      function(str, len)         { return str.slice(0, len);                       },
        String$$ReplaceCharAt: function(str, i, ch) {
            return str.slice(0, i) + String.fromCharCode(ch) + str.slice(i + 1);
        },
        String$$StartsWith: function(str, startsWithText, caseSensitive) {
            return new RegExp('^' + ags.util.regexEscape(startsWithText), caseSensitive ? 'g' : 'gi').test(str);
        },
        String$$EndsWith: function(str, endsWithText, caseSensitive) {
            return new RegExp(ags.util.regexEscape(endsWithText) + '$', caseSensitive ? 'g' : 'gi').test(str);
        },
        String$$Replace: function(str, replaceText, withText, caseSensitive) {
            if (caseSensitive) return str.replace(replaceText, withText);
            return str.replace(new RegExp(ags.util.regexEscape(replaceText), 'gi'), withText);
        },
        String$$CompareTo: function(str, other, caseSensitive) {
            if (str === other) return 0;
            if (!caseSensitive) {
                str = str.toLowerCase();
                other = other.toLowerCase();
            }
            if (str < other) {
                return -1;
            }
            else if (str > other) {
                return 1;
            }
            else {
                return 0;
            }
        },
        
        // Time-Related Utilities
        DateTime$$get_Now:        function()   { return new Date();                },
        DateTime$$get_Year:       function(dt) { return dt.getFullYear();          },
        DateTime$$get_Month:      function(dt) { return dt.getMonth() + 1;         },
        DateTime$$get_DayOfMonth: function(dt) { return dt.getDate();              },
        DateTime$$get_Hour:       function(dt) { return dt.getHours();             },
        DateTime$$get_Minute:     function(dt) { return dt.getMinutes();           },
        DateTime$$get_Second:     function(dt) { return dt.getSeconds();           },
        DateTime$$get_RawTime:    function(dt) { return (dt.valueOf() / 1000) | 0; },
        SetTimer: function(timerNumber, loopTimeout) {
            /* TODO */
            return;
        },
        IsTimerExpired: function(timerNumber) {
            /* TODO */
            return 0;
        },
        Wait: util.blockingFunction(['loops'], function($ctx, $stk, $vars) {
            var loops = $vars["loops"];
            var engine = $ctx.engine;
            var game = engine.game.game;
            if (game.skipping_cutscene) {
                return $ctx.finish();
            }
            $ctx.queueCall(engine.waitUntil, [loops, false, false]);
            return $ctx.finish();
        }),
        WaitKey: util.blockingFunction(['loops'], function($ctx, $stk, $vars) {
            switch($ctx.entryPoint) {
                case 0:
                    var loops = $vars["loops"];
                    var engine = $ctx.engine;
                    var game = engine.game.game;
                    if (game.skipping_cutscene) {
                        return $ctx.finish(1);
                    }
                    $ctx.beginCall(engine.waitUntil, [loops, false, true]);
                    return $ctx.nextEntryPoint(1);
                case 1:
                    return $ctx.finish($stk.pop());
            }
        }),
        WaitMouseKey: util.blockingFunction(['loops'], function($ctx, $stk, $vars) {
            switch($ctx.entryPoint) {
                case 0:
                    var loops = $vars["loops"];
                    var engine = $ctx.engine;
                    var game = engine.game.game;
                    if (game.skipping_cutscene) {
                        return $ctx.finish(1);
                    }
                    $ctx.beginCall(engine.waitUntil, [loops, true, false]);
                    return $ctx.nextEntryPoint(1);
                case 1:
                    return $ctx.finish($stk.pop());
            }
        }),
        SetGameSpeed: function(fps) { this.renderer.startFrames(fps); },
        GetGameSpeed: function()    { return this.framesPerSecond;    },
        
        // Skippable Cutscenes
        Game$$get_InSkippableCutscene: function() { return this.game.game.in_cutscene; },
        Game$$get_SkippingCutscene: function() { return this.game.game.skipping_cutscene; },
        startSkippingCutscene: function() {
            this.game.game.skipping_cutscene = 1;
            $(this).trigger('skipCutscene');
        },
        StartCutscene: function(skipType) {
            var engine = this;
            var game = this.game.game;
            game.in_cutscene = 1;
            if (skipType === 1 /* eSkipESCOnly */ || skipType === 5 /* eSkipESCOrRightButton */) {
                var escListener = function(e, keyCode) {
                    if (keyCode === 27 /* eKeyEscape */) {
                        engine.startSkippingCutscene();
                    }
                };
                $(engine).bind('keyDown', escListener);
                $(engine).bind('endCutscene', function(e) {
                    $(engine).unbind('keyDown', escListener);
                });
            }
            if (skipType === 2 /* eSkipAnyKey */ || skipType === 4 /* eSkipAnyKeyOrMouseClick */) {
                var keyListener = function(e, keyCode) {
                    engine.startSkippingCutscene();
                };
                $(engine).bind('keyDown', keyListener);
                $(engine).bind('endCutscene', function(e) {
                    $(engine).unbind('keyDown', keyListener);
                });
            }
            if (skipType === 3 /* eSkipMouseClick */ || skipType === 4 /* eSkipAnyKeyOrMouseClick */) {
                var mouseListener = function(e, mouseButton) {
                    engine.startSkippingCutscene();
                };
                $(engine).bind('mouseDown', mouseListener);
                $(engine).bind('endCutscene', function(e) {
                    $(engine).unbind('keyDown', keyListener);
                });
            }
            if (skipType === 5 /* eSkipESCOrRightButton */) {
                var rightButtonListener = function(e, mouseButton) {
                    if (mouseButton === 2 /* RIGHT */) {
                        engine.startSkippingCutscene();
                    }
                };
                $(engine).bind('mouseDown', rightButtonListener);
                $(engine).bind('endCutscene', function(e) {
                    $(engine).unbind('keyDown', rightButtonListener);
                });
            }
            $(this).trigger('startCutscene');
        },
        EndCutscene: function() {
            var game = this.game.game;
            var skipped = game.skipping_cutscene;
            game.in_cutscene = 0;
            game.skipping_cutscene = 0;
            $(this).trigger('endCutscene');
            return skipped;
        },
        SkipUntilCharacterStops: function(chrNumber) { /* TODO */ },
        
        // Translation
        GetTranslation: function(originalText) { /* TODO */ return null; },
        IsTranslationAvailable: function() { /* TODO */ return 0; },
        Game$$ChangeTranslation: function(newTranslationName) { /* TODO */ },
        Game$$get_TranslationFilename: function() { /* TODO */ return 0; },
        
        // Files
        File$$Delete: function(filename) { return this.fileSystem.deleteFile(filename) | 0; },
        File$$Exists: function(filename) { return this.fileSystem.fileExists(filename) | 0; },
        File$$Open: function(filename, fileMode) {
            switch(fileMode) {
                case 1: return this.fileSystem.openFile(filename, 'read') || null;
                case 2: return this.fileSystem.openFile(filename, 'write') || null;
                case 3: return this.fileSystem.openFile(filename, 'append') || null;
            }
            return null;
        },
        File$$Close:           function(f)   { f.close();                      },
        File$$ReadInt:         function(f)   { return f.readInt() | 0;         },
        File$$ReadRawChar:     function(f)   { return f.readRawChar() | 0;     },
        File$$ReadRawInt:      function(f)   { return f.readRawInt() | 0;      },
        File$$ReadStringBack:  function(f)   { return f.readString() || null;  },
        File$$ReadRawLineBack: function(f)   { return f.readRawLine() || null; },
        File$$WriteInt:        function(f,v) { f.writeInt(v);                  },
        File$$WriteString:     function(f,v) { f.writeString(v);               },
        File$$WriteRawLine:    function(f,v) { f.writeRawLine(v);              },
        File$$WriteRawChar:    function(f,v) { f.writeRawChar(v);              },
        File$$get_EOF:         function(f)   { return f.isEof() | 0;           },
        File$$get_Error:       function(f)   { return f.isError() | 0;         },
        
        // Events
        ClaimEvent: function() { /* TODO */ },

        /*** Scripting API: Game-Wide Settings and Information ***/
        
        // Context Information and Settings
        Game$$get_FileName: function() { return this.game.filename || ''; },
        System$$get_OperatingSystem: function() {
            var systemName = this.sniffOperatingSystem();
            switch(systemName) {
                case "dos":     return 1 /* eOSDOS */;
                case "windows": return 2 /* eOSWindows */;
                case "linux":   return 3 /* eOSLinux */;
                case "macos":   return 4 /* eOSMacOS */;
                default:        return 2 /* eOSWindows */;
            }
        },
        System$$get_SupportsGammaControl: function() { /* TODO */ return 0; },
        System$$get_Version: function() { return this.versionString; },
        SetMultitaskingMode: function(mode) { /* TODO */ },
        
        // Game Settings
        SetGameOption: function(opt, v)   { /* TODO */ },
        GetGameOption: function(opt)      { /* TODO */ return 0; },
        Game$$get_Name: function()        { /* TODO */ return null; },
        Game$$set_Name: function(v)       { /* TODO */ },
        Game$$get_NormalFont: function()  { /* TODO */ return 0; },
        Game$$set_NormalFont: function(v) { /* TODO */ },
        Game$$get_SpeechFont: function()  { /* TODO */ return 0; },
        Game$$set_SpeechFont: function(v)  { /* TODO */ },
        SetTextWindowGUI: function(guiNumber) { /* TODO */ },
        Game$$get_IgnoreUserInputAfterTextTimeoutMs: function() { /* TODO */ return 0; },
        Game$$set_IgnoreUserInputAfterTextTimeoutMs: function(v) { /* TODO */ },
        Game$$get_MinimumTextDisplayTimeMs: function() { /* TODO */ return 0; },
        Game$$set_MinimumTextDisplayTimeMs: function(v) { /* TODO */ },
        Game$$get_TextReadingSpeed: function() { /* TODO */ return 0; },
        Game$$set_TextReadingSpeed: function(v) { /* TODO */ },
        Character$$SetAsPlayer: function(chr) { /* TODO */ },
        Game$$get_UseNativeCoordinates: function() { /* TODO */ return 0; },
        GiveScore: function(points) { /* TODO */ },
        SetSpeechStyle: function(style) { /* TODO */ },
        SetVoiceMode: function(mode) { /* TODO */ },
        SetSkipSpeech: function(skipFlag) { /* TODO */ },
        ResetRoom:                            function(roomNumber) { },
        HasPlayerBeenInRoom:                  function(roomNumber) { },
        
        // Variable and Message Stores
        GetGraphicalVariable:      function(name)    { /* TODO */ return 0;    },
        SetGraphicalVariable:      function(name, v) { /* TODO */              },
        Game$$geti_GlobalMessages: function(i)       { /* TODO */ return null; },
        Game$$geti_GlobalStrings:  function(i)       { /* TODO */ return null; },
        Game$$seti_GlobalStrings:  function(i,v)     { /* TODO */              },
        GetGlobalInt:              function(i)       { /* TODO */ return 0;    },
        SetGlobalInt:              function(i, v)    { /* TODO */              },
        
        // Sprites
        Game$$geti_SpriteHeight: function(i) { /* TODO */ return 0; },
        Game$$geti_SpriteWidth:  function(i) { /* TODO */ return 0; },
        
        // Views, Loops and Frames
        Game$$get_ViewCount:            function()      { return this.game.views.length - 1;                },
        Game$$GetLoopCountForView:      function(v)     { return this.game.views[v].loops.length;           },
        Game$$GetRunNextSettingForLoop: function(v,l)   { return this.game.views[v].loops[l].runNext | 0;   },
        Game$$GetFrameCountForLoop:     function(v,l)   { return this.game.views[v].loops[l].frames.length; },
        Game$$GetViewFrame:             function(v,l,f) { /* TODO */ return null;                           },
        ViewFrame$$get_Flipped:         function(vf)    { return vf.flipped;                                },
        ViewFrame$$get_Frame:           function(vf)    { return vf.number;                                 },
        ViewFrame$$get_Graphic:         function(vf)    { return vf.graphic;                                },
        ViewFrame$$set_Graphic:         function(vf,g)  { vf.graphic = g;                                   },
        ViewFrame$$get_LinkedAudio:     function(vf)    { return vf.linkedAudio;                            },
        ViewFrame$$set_LinkedAudio:     function(vf,au) { vf.linkedAudio = au;                              },
        ViewFrame$$get_Loop:            function(vf)    { return vf.l.number;                               },
        ViewFrame$$get_Sound:           function(vf)    { return vf.sound;                                  },
        ViewFrame$$set_Sound:           function(vf,s)  { vf.sound = s;                                     },
        ViewFrame$$get_Speed:           function(vf)    { return vf.delay;                                  },
        ViewFrame$$get_View:            function(vf)    { return vf.v.number;                               },

        // Fonts
        Game$$get_FontCount: function() { /* TODO */ return 0; },

        // Misc.
        System$$get_ColorDepth:           function()  { /* TODO */ return 0; },
        System$$get_HardwareAcceleration: function()  { /* TODO */ return 0; },
        System$$get_ScreenHeight:         function()  { /* TODO */ return 0; },
        System$$get_VSync:                function()  { /* TODO */ return 0; },
        System$$set_VSync:                function(v) { /* TODO */           },
        System$$get_Windowed:             function()  { /* TODO */           },
        
        // Saving, Restoring, Restarting and Quitting
        saveGamePath: function(slot) {
            return "$SAVEGAMEDIR$/agssave." + (slot<100?"0":"") + (slot<10?"0":"") + slot;
        },
        RestartGame: function() {
            this.RestoreGameSlot(999);
        },
        SetRestartPoint: function() {
            this.SaveGameSlot(999);
        },
        SaveGameSlot: function(slot, description) {
            var file = this.fileSystem.openFile(this.saveGamePath(slot), 'write');
            if (!file) {
                alert("Sorry! Saving is not done yet.");
                return;
            }
            var saveData = {description:description};
            saveData.engine = this.save();
            saveData.game = this.game.save();
            saveData.room = this.room.save();
            file.writeRawString(JSON.stringify(saveData));
            file.close();
        },
        save: function() {
            var data = {};
            return data;
        },
        RestoreGameSlot: function(slot) {
            var file = this.fileSystem.openFile(this.saveGamePath(slot), 'read');
            if (!file) {
                alert("Save game " + slot + " not found");
                return;
            }
            var saveData = JSON.parse(file.readAllText());
            file.close();
            this.blockingThread.queueCall(util.blockingFunction([], function($ctx, $stk, $vars) {
                switch($ctx.entryPoint) {
                    case 0:
                        $ctx.queueCall($ctx.engine.FadeOut, [30]);
                        return $ctx.nextEntryPoint(1);
                    case 1:
                        $ctx.queueCall($ctx.engine.Wait, [40]);
                        return $ctx.nextEntryPoint(2);
                    case 2:
                        $ctx.queueCall($ctx.engine.FadeIn, [30]);
                        return $ctx.nextEntryPoint(3);
                    case 3:
                        return $ctx.finish();
                }
            }));
        },
        DeleteSaveSlot: function(slot) {
            return this.fileSystem.deleteFile(this.saveGamePath(slot)) | 0;
        },
        Game$$GetSaveSlotDescription: function(slot) {
            if (!this.fileSystem.fileExists(this.saveGamePath(slot))) {
                return null;
            }
            var file = this.fileSystem.openFile(this.saveGamePath(slot), 'read');
            var saveData = JSON.parse(file.readAllText());
            file.close();
            return file.description;
        },
        Game$$SetSaveGameDirectory:   function(dir)        { /* TODO */              },
        QuitGame: function(ask) {
            /* TODO: ask */
            $(this).trigger("quit");
        },
        AbortGame: function(message, fmtArgs) {
            message = ags.util.format(message, fmtArgs);
            $(this).trigger("quit", [message]);
        },
        
        // Mouse and Keyboard Input
        Mouse$$Update: function() {
            var mouse = this.mouse;
            mouse.x = mouse.nextX;
            mouse.y = mouse.nextY;
            mouse.onScreen = mouse.nextOnScreen;
        },
        Mouse$$IsButtonDown:    function(btn)                      { /* TODO */ return 0; },
        Mouse$$SetBounds:       function(left, top, right, bottom) { /* TODO (probably not) */ },
        Mouse$$SetPosition:     function(x, y)                     { /* TODO (probably not) */ },
        System$$get_NumLock:    function()                         { /* TODO */ return 0; },
        System$$get_ScrollLock: function()                         { /* TODO */ return 0; },
        System$$get_CapsLock:   function()                         { /* TODO */ return 0; },
        IsKeyPressed:           function(keycode)                  { /* TODO */ return 0; },
        
        /*** Scripting API: Characters ***/
        
        // Character Properties
        Game$$get_CharacterCount: function()    { return this.game.character.length; },
        Character$$get_ID:        function(chr) { return chr.number;                  },
        Character$$AddInventory: function(chr, item, addAtIndex) {
            /* TODO */
            if (addAtIndex === 31998 /* SCR_NO_VALUE */) {
            }
            return 0;
        },
        Character$$LoseInventory: function(chr, item) { /* TODO */ },
        Character$$geti_InventoryQuantity: function(chr, itemNumber) {
            /* TODO */
            return 0;
        },
        Character$$HasInventory: function(chr, item) { /* TODO */ return 0; },
        Character$$get_ActiveInventory: function(chr)       { /* TODO */ return null; },
        Character$$set_ActiveInventory: function(chr, item) { /* TODO */             },
        Character$$get_Room: function(chr) { /* TODO */ return 0; },
        Character$$ChangeRoom: function(chr, room, x, y) {
            /* TODO */
            chr.room = room;
            if (x !== 31998 /* SCR_NO_VALUE */) {
                chr.x = x;
            }
            if (y !== 31998 /* SCR_NO_VALUE */) {
                chr.y = y;
            }
            if (chr === this.game.player) {
                this.inDialog = false;
                this.loadRoom(room);
            }
            else if (room === this.room.number) {
                chr.deliverSprite();
            }
            else {
                chr.withdrawSprite();
            }
        },
        Character$$ChangeRoomAutoPosition: function(chr, room, position) { /* TODO */ },
        Character$$get_PreviousRoom: function(chr) { /* TODO */ return 0; },
        Character$$get_Clickable: function(chr) { /* TODO */ return 0; },
        Character$$set_Clickable: function(chr, v) { /* TODO */ },
        Character$$get_Name: function(chr) { /* TODO */ return null; },
        Character$$set_Name: function(chr, v) { /* TODO */ },
        
        // Character Movement and Direction
        Character$$Walk: util.blockingFunction(["chr","x","y","blockStyle","walkWhere"], function($ctx, $stk, $vars) {
            var chr = $vars["chr"], x = $vars["x"], y = $vars["y"], blockStyle = $vars["blockStyle"], walkWhere = $vars["walkWhere"];
            switch($ctx.entryPoint) {
                case 0:
                    var engine = $ctx.engine;
                    // TODO: use turn-before-walking settings
                    $ctx.queueCall(engine.Character$$FaceLocation, [chr, x, y]);
                    return $ctx.nextEntryPoint(1);
                case 1:
                    // TODO: use animation delay, use-first-frame settings
                    $ctx.queueCall($ctx.engine.animateSprite, [
                        chr,
                        1 /* ignore frame 0 */,
                        chr.loop,
                        chr.animationDelay, /* delay */
                        1 /* eRepeat */,
                        920 /* eNoBlock */,
                        1062 /* eForwards */]);
                    $ctx.queueCall($ctx.engine.spriteMove, [chr.sprite, x, y, chr.movementSpeed, blockStyle, walkWhere, true]);
                    $ctx.queueCall($ctx.engine.Character$$set_Frame, [chr, 0]);
                    return $ctx.finish(0);
            }
        }),
        spriteMove: util.blockingFunction(["spr","x","y","speed","blockStyle","walkWhere","movementLinked"], function($ctx, $stk, $vars) {
            var spr = $vars["spr"], x = $vars["x"], y = $vars["y"], speed = $vars["speed"], blockStyle = $vars["blockStyle"], walkWhere = $vars["walkWhere"], movementLinked = $vars["movementLinked"];
            // TODO: honour walkWhere and movementLinkedToAnmation
            var engine = $ctx.engine;
            if (engine.game.game.skipping_cutscene) {
                spr.setParams({
                    x: x,
                    y: y
                });
                return $ctx.finish(0);
            }
            if (blockStyle === 919 /* eBlock */) {
                $ctx.blocking = true;
                // the arrival handler is not unbound here because skipMoving()
                // should actually trigger it
                var skipHandler = function(e) {
                    $(this).unbind(e);
                    spr.skipMoving();
                };
                $(engine).bind('skipCutscene', skipHandler);
                $(spr).bind('arrived', function(e) {
                    $(engine).unbind('skipCutscene', skipHandler);
                    $(this).unbind(e);
                    $ctx.blocking = false;
                });
            }
            spr.startMovingTowards(x, y, speed, movementLinked);
            return $ctx.finish(0);
        }),
        Character$$Move: util.blockingFunction(["chr","x","y","blockStyle","walkWhere"], function($ctx, $stk, $vars) {
            var chr = $vars["chr"], x = $vars["x"], y = $vars["y"], blockStyle = $vars["blockStyle"], walkWhere = $vars["walkWhere"];
            var engine = $ctx.engine;
            if (chr.sprite) {
                $ctx.queueCall(engine.spriteMove, [chr.sprite, x, y, chr.movementSpeed, blockStyle, walkWhere, false]);
            }
            return $ctx.finish(0);
        }),
        Character$$WalkStraight:        function(chr, x, y, blockStyle)            { /* TODO */ }, 
        Character$$AddWaypoint:         function(chr, x, y)                        { /* TODO */ },
        Character$$FaceLocation: function(chr, x, y, blockStyle) {
            /* TODO: Turn animation */
            var ang = Math.atan2(y - chr.sprite.getParam("y"), x - chr.sprite.getParam("x")) / Math.PI;
            var loop;
            /* TODO: diagonal directions */
            if (ang < -0.75 || ang >= 0.75) {
                // left
                loop = 1;
            }
            else if (ang < -0.25) {
                // down
                loop = 0;
            }
            else if (ang < 0.25) {
                // right
                loop = 2;
            }
            else /* if (ang < 0.75) */ {
                // up
                loop = 3;
            }
            if (loop < chr.normalView.loops.length) {
                this.Character$$set_Loop(chr, loop);
            }
        },
        Character$$FaceCharacter: function(chr, otherChr, blockStyle) {
            /* TODO: Turn animation */
            this.Character$$FaceLocation(chr, otherChr.x, otherChr.y, blockStyle);
        },
        Character$$FaceObject: function(chr, obj, blockStyle) {
            /* TODO: Turn animation */
            this.Character$$FaceLocation(chr, obj.x, obj.y, blockStyle);
        },
        Character$$PlaceOnWalkableArea: function(chr)                              { /* TODO */ },
        Character$$StopMoving:          function(chr)                              { /* TODO */ },
        Character$$get_Moving: function(chr) { /* TODO */ return 0; },
        Character$$get_x: function(chr) { /* TODO */ return chr.sprite.getParam("x"); },
        Character$$set_x: function(chr, x) {
            chr.x = x;
            if (chr.sprite) {
                chr.sprite.setParam("x", x);
            }
        },
        Character$$get_y: function(chr) { /* TODO */ return chr.sprite.getParam("y"); },
        Character$$set_y: function(chr, y) {
            chr.y = y;
            if (chr.sprite) {
                chr.sprite.setParam("y", y);
            }
        },
        Character$$get_z: function(chr) { /* TODO */ return 0; },
        Character$$set_z: function(chr, v) { /* TODO */ },

        // Character Appearance and Animation
        Character$$get_View:        function(chr)                         { /* TODO */ return 0; },
        Character$$ChangeView: function(chr, view) {
            /* TODO */
            chr.view = view;
            var viewObj = this.game.views[view];
            chr.normalView = viewObj;
            this.Character$$set_Loop(chr, chr.loop < viewObj.loops.length ? chr.loop : 0);
        },
        Character$$LockView:        function(chr, view)                   { /* TODO */           },
        Character$$LockViewAligned: function(chr, view, loop, alignment)  { /* TODO */           },
        Character$$LockViewOffset:  function(chr, view, xOffset, yOffset) { /* TODO */           },
        Character$$LockViewFrame:   function(chr, view, loop, frame)      { /* TODO */           },
        Character$$get_SpeechView:  function(chr)                         { /* TODO */ return 0; },
        Character$$set_SpeechView:  function(chr, v)                      { /* TODO */           },
        Character$$get_BlinkView:   function(chr)                         { /* TODO */ return 0; },
        Character$$set_BlinkView:   function(chr, v)                      { /* TODO */           },
        Character$$RemoveTint:      function(chr)                         { /* TODO */           },
        Character$$get_WalkSpeedX: function(chr) { /* TODO */ return 0; },
        Character$$get_WalkSpeedY: function(chr) { /* TODO */ return 0; },
        Character$$SetWalkSpeed: function(chr, x, y) {
            /* TODO */
            chr.movementSpeed = x;
        },
        Character$$SetIdleView:     function(chr, view, delay)            { /* TODO */           },
        Character$$Tint:            function(chr, r,g,b,sat,lum)          { /* TODO */           },
        Character$$UnlockView:      function(chr)                         { /* TODO */           },
        Character$$get_Animating:   function(chr)                         { /* TODO */ return 0; },
        Character$$get_AnimationSpeed: function(chr) { /* TODO */ return 0; },
        Character$$set_AnimationSpeed: function(chr, v) { /* TODO */ },
        Character$$get_Baseline: function(chr) { /* TODO */ return 0; },
        Character$$set_Baseline: function(chr, v) {
            /* TODO */
            chr.baseline = v;
            if (chr.sprite) {
                chr.sprite.setParam("baseline", v);
            }
        },
        Character$$get_BlinkInterval: function(chr) { /* TODO */ return 0; },
        Character$$set_BlinkInterval: function(chr, v) { /* TODO */ },
        Character$$get_BlinkWhileThinking: function(chr) { /* TODO */ return 0; },
        Character$$set_BlinkWhileThinking: function(chr, v) { /* TODO */ },
        Character$$get_DiagonalLoops: function(chr) { /* TODO */ return 0; },
        Character$$set_DiagonalLoops: function(chr, v) { /* TODO */ },
        Character$$get_NormalView: function(chr) { /* TODO */ return 0; },
        Character$$get_Frame: function(chr) {
            return chr.frame || 0;
        },
        Character$$set_Frame: function(chr, frame) {
            chr.frame = frame;
            if (chr.sprite) {
                $(chr.sprite).trigger('stopAnimating');
                chr.sprite.setParam("image", this.game.views[chr.view].loops[chr.loop].frames[frame].image);
            }
        },
        Character$$get_HasExplicitTint:           function(chr) { /* TODO */ return 0; },
        Character$$get_IdleView:                  function(chr)   { /* TODO */ return 0; },
        Character$$get_IgnoreLighting:            function(chr)   { /* TODO */ return 0; },
        Character$$set_IgnoreLighting:            function(chr,v) { /* TODO */ },
        Character$$get_IgnoreScaling:             function(chr)   { /* TODO */ return 0; },
        Character$$set_IgnoreScaling:             function(chr,v) { /* TODO */ },
        Character$$get_Scaling: function(chr) { /* TODO */ return 0; },
        Character$$set_Scaling: function(chr, v) { /* TODO */ },
        Character$$get_SpeakingFrame: function(chr) { /* TODO */ return 0; },
        Character$$get_SpeechAnimationDelay: function(chr) { /* TODO */ return 0; },
        Character$$set_SpeechAnimationDelay: function(chr, v) { /* TODO */ },
        Character$$get_SpeechColor: function(chr) { /* TODO */ return 0; },
        Character$$set_SpeechColor: function(chr, v) { /* TODO */ },
        Character$$get_ThinkView: function(chr) { /* TODO */ return 0; },
        Character$$set_ThinkView: function(chr, v) { /* TODO */ },
        Character$$get_Loop:                      function(chr)   { return chr.loop; },
        Character$$set_Loop: function(chr, loop) {
            /* TODO */
            chr.loop = loop;
            chr.frame = 0;
            if (chr.sprite) {
                var frame = chr.normalView.loops[loop].frames[0];
                chr.sprite.setParam("image", frame.image);
                chr.sprite.setParam("flipped", frame.flipped);
            }
        },
        Character$$get_Transparency: function(chr) { /* TODO */ return 0; },
        transparencyToAlpha: function(t) {
            return 1 - (Math.max(0, Math.min(100, t)) / 100);
        },
        Character$$set_Transparency: function(chr, t) {
            /* TODO */
            chr.transparency = t;
            if (chr.sprite) {
                chr.sprite.setParam('alpha', this.transparencyToAlpha(t));
            }
        },
        Character$$get_TurnBeforeWalking: function(chr) { /* TODO */ return 0; },
        Character$$set_TurnBeforeWalking: function(chr, v) { /* TODO */ },
        Character$$get_IgnoreWalkbehinds:         function(chr)   { /* TODO */ return 0; },
        Character$$set_IgnoreWalkbehinds:         function(chr,v) { /* TODO */ },
        Character$$get_ManualScaling:             function(chr)   { /* TODO */ return 0; },
        Character$$set_ManualScaling:             function(chr,v) { /* TODO */ },
        Character$$get_MovementLinkedToAnimation: function(chr)   { /* TODO */ return 0; },
        Character$$set_MovementLinkedToAnimation: function(chr,v) { /* TODO */ },
        Character$$get_ScaleMoveSpeed: function(chr) { /* TODO */ return 0; },
        Character$$set_ScaleMoveSpeed: function(chr, v) { /* TODO */ },
        Character$$get_ScaleVolume: function(chr) { /* TODO */ return 0; },
        Character$$set_ScaleVolume: function(chr, v) { /* TODO */ },
        animateSprite: util.blockingFunction(
            ["owner", "firstFrame", "loop","delay","repeatStyle","blockStyle","direction"],
            function($ctx, $stk, $vars) {
                /* TODO */
                var chr = $vars["owner"], firstFrame = $vars["firstFrame"],
                    loop = $vars["loop"], delay = $vars["delay"], repeatStyle = $vars["repeatStyle"],
                    blockStyle = $vars["blockStyle"], direction = $vars["direction"];
                
                chr.loop = loop;
                
                var engine = $ctx.engine;
                var loop = engine.game.views[chr.view].loops[loop];
                var frames = loop.frames;
                if (frames.length === 1) {
                    return $ctx.finish();
                }
                
                var sprite = chr.sprite;
                
                var frame, frame_offset, totalDelay = 0;
                
                if (direction === 1063 /* eBackwards */) {
                    frame = frames.length - 1;
                    frame_offset = -1;
                }
                else {
                    frame = firstFrame;
                    frame_offset = 1;
                }
                
                var updateAnimation;
                
                var stopped = false;
                if (repeatStyle === 1 /* eRepeat */) {
                    var updateAnimation = function(e) {
                        if (stopped) {
                            $(this).unbind(e);
                            return;
                        }
                        if (frame >= frames.length) {
                            frame = firstFrame;
                        }
                        if (totalDelay > 0) {
                            totalDelay--;
                        }
                        else {
                            totalDelay = delay /* + chr.animationDelay */ + frames[frame].delay;
                            sprite.setParam("image", frames[frame].image);
                            frame += frame_offset;
                            chr.frame = Math.max(firstFrame, Math.min(frames.length-1, frame));
                        }
                    };
                    $(chr.sprite).bind('stopAnimating', function(e) {
                        stopped = true;
                        $(engine).unbind('frame', updateAnimation);
                        $(this).unbind(e);
                    });
                    $(engine).bind('frame', updateAnimation);
                    return $ctx.finish(0);
                }
                
                if (engine.game.game.skipping_cutscene) {
                    if (direction === 1063 /* eBackwards */) {
                        chr.sprite.setParam("image", frames[firstFrame].image);
                    }
                    else {
                        chr.sprite.setParam("image", frames[frames.length-1].image);
                    }
                    return $ctx.finish();
                }
                
                var skipAnimation, stopAnimation;
                updateAnimation = function(e) {
                    if (stopped) {
                        $(this).unbind(e);
                        return;
                    }
                    if (frame < firstFrame || frame >= frames.length) {
                        $(engine).unbind('skipCutscene', skipAnimation);
                        $(sprite).unbind('stopAnimating', stopAnimation);
                        $ctx.blocking = false;
                        $(this).unbind(e);
                        return;
                    }
                    if (totalDelay > 0) {
                        totalDelay--;
                    }
                    else {
                        totalDelay = delay /* + chr.animationDelay */ + frames[frame].delay;
                        chr.sprite.setParam("image", frames[frame].image);
                        frame += frame_offset;
                        chr.frame = Math.max(firstFrame, Math.min(frames.length-1, frame));
                    }
                };
                $(engine).bind('frame', updateAnimation);
                $(sprite).bind('stopAnimating', function(e) {
                    stopped = true;
                    $(engine).unbind('frame', updateAnimation);
                    $(this).unbind(e);
                });
                
                skipAnimation = function(e) {
                    $ctx.blocking = false;
                    chr.sprite.setParam("image", frames[frames.length-1].image);
                    $(engine).unbind('frame', updateAnimation);
                    $(this).unbind(e);
                };
                $(engine).bind('skipCutscene', skipAnimation);
                
                $ctx.blocking = true;
                
                return $ctx.finish();
            }
        ),
        Character$$Animate: util.blockingFunction(
            ["chr","loop","delay","repeatStyle","blockStyle","direction"],
            function($ctx, $stk, $vars) {
                var chr = $vars["chr"], loop = $vars["loop"], delay = $vars["delay"], repeatStyle = $vars["repeatStyle"],
                    blockStyle = $vars["blockStyle"], direction = $vars["direction"];
                $ctx.queueCall($ctx.engine.animateSprite, [chr, 0, loop, delay, repeatStyle, blockStyle, direction]);
                return $ctx.finish();
            }),

        // Character Collisions
        Character$$get_Solid: function(chr) { /* TODO */ return 0; },
        Character$$set_Solid: function(chr, v) { /* TODO */ },
        Character$$get_BlockingHeight: function(chr) { /* TODO */ return 0; },
        Character$$set_BlockingHeight: function(chr, v) { /* TODO */ },
        Character$$get_BlockingWidth: function(chr) { /* TODO */ return 0; },
        Character$$set_BlockingWidth: function(chr, v) { /* TODO */ },
        Character$$IsCollidingWithChar:   function(chr, otherChr) { /* TODO */ return 0; },
        Character$$IsCollidingWithObject: function(chr, obj)      { /* TODO */ return 0; },
      
        // Character Dialogue
        calculateDisplayLength: function(message) {
            return (Math.floor(message.length / this.game.game.text_speed) + 1) * this.framesPerSecond;
        },
        // returns: 1 if user intervened, 0 if a timeout occurred
        waitUntil: util.blockingFunction(["framesElapsed", "mouseButton", "key"], function($ctx, $stk, $vars) {
            var framesElapsed = $vars["framesElapsed"], mouseButton = $vars["mouseButton"], key = $vars["key"];
            $ctx.blocking = true;
            var engine = $ctx.engine;
            var wait = {};
            $(wait).bind('waitOver', function(e) {
                $ctx.blocking = false;
                $(this).unbind(e);
            });
            // cutscene skipping
            var skipListener = function() {
                $ctx.returnValue = 1;
                $(wait).trigger('waitOver');
            };
            $(wait).bind('waitOver', function() {
                $(engine).unbind('skipCutscene', skipListener);
            });
            $(engine).bind('skipCutscene', skipListener);
            if (typeof framesElapsed === 'number') {
                // number of frames to wait
                var countdown = function(e) {
                    if (--framesElapsed <= 0) {
                        $ctx.returnValue = 0;
                        $(wait).trigger('waitOver');
                    }
                };
                $(wait).bind('waitOver', function(e) {
                    $(engine).unbind('frame', countdown);
                });
                $(engine).bind('frame', countdown);
            }
            if (key === true) {
                // any key
                var anyKeyListener = function(e, keyCode) {
                    // ignore ctrl/alt/shift
                    if (keyCode >= 16 && keyCode <= 18) {
                        return;
                    }
                    $ctx.returnValue = 1;
                    $(wait).trigger('waitOver');
                };
                $(wait).bind('waitOver', function(e) {
                    $(engine).unbind('keyDown', anyKeyListener);
                });
                $(engine).bind('keyDown', anyKeyListener);
            }
            else if (typeof key === 'number') {
                // specific key
                var anyKeyListener = function(e, kc) {
                    if (kc === key) {
                        $ctx.returnValue = 1;
                        $(wait).trigger('waitOver');
                    }
                };
                $(wait).bind('waitOver', function(e) {
                    $(engine).unbind('keyDown', anyKeyListener);
                });
                $(engine).bind('keyDown', anyKeyListener);
            }
            if (mouseButton === true) {
                // any mouse button
                var anyMouseButtonListener = function(e) {
                    $ctx.returnValue = 1;
                    $(wait).trigger('waitOver');
                };
                $(wait).bind('waitOver', function(e) {
                    $(engine).unbind('mouseDown', anyMouseButtonListener);
                });
                $(engine).bind('mouseDown', anyMouseButtonListener);
            }
            else if (typeof mouseButton === 'number') {
                // specific mouse button
                var mouseButtonListener = function(e, btn) {
                    if (btn === mouseButton) {
                        $ctx.returnValue = 1;
                        $(wait).trigger('waitOver');
                    }
                };
                $(wait).bind('waitOver', function(e) {
                    $(engine).unbind('mouseDown', mouseButtonListener);
                });
                $(engine).bind('mouseDown', mouseButtonListener);
            }
            return $ctx.finish(0);
        }),
        Character$$Say: util.blockingFunction(["chr", "message", "fmtArgs"], function($ctx, $stk, $vars) {
            var chr = $vars["chr"], message = $vars["message"], fmtArgs = $vars["fmtArgs"];
            message = ags.util.format(message, fmtArgs);
            var engine = $ctx.engine;
            var game = engine.game.game;
            if (game.skipping_cutscene) {
                return $ctx.finish();
            }
            var renderer = engine.renderer;
            var font = $ctx.engine.game.fonts[3];
            var lines = font.splitLines(message, 230);
            if (lines.length === 1 && lines[0] === '...') {
                lines[0] = '';
            }
            var totalWidth = 0;
            var totalHeight = font.def.lineHeight * lines.length;
            for (var i = 0; i < lines.length; i++) {
                var wid = font.stringWidth(lines[i]);
                lines[i] = {text:lines[i], width:wid};
                totalWidth = Math.max(totalWidth, wid);
            }
            var x, y;
            if (chr.sprite) {
                x = Math.max(5, Math.min(315 - totalWidth, chr.sprite.getParam("x") - Math.floor(totalWidth/2)));
                y = Math.max(5,
                    Math.min(215 - totalHeight,
                    chr.sprite.getParam("y") - $ctx.engine.game.def.numberedImages[chr.sprite.getParam("image")].h - totalHeight));
            }
            else {
                x = Math.floor((320 - totalWidth) / 2);
                y = Math.floor((240 - totalHeight) / 2);
            }
            var cimage = chr.sprite ? chr.sprite.getParam("image") : 30;
            for (var i = 0; i < lines.length; i++) {
                lines[i] = renderer.createTextOverlay(
                    x + Math.floor((totalWidth - lines[i].width)/2),
                    y + font.def.lineHeight * i,
                    totalWidth,
                    3,
                    chr.def.speechColor,
                    lines[i].text);
            }
            var key = game.skip_speech_specific_key || true;
            switch(engine.game.skip_speech) {
                case 0 /* mouse, key or timer */:
                    $ctx.queueCall(engine.waitUntil, [engine.calculateDisplayLength(message), true, key]);
                    break;
                case 1 /* key or timer */:
                    $ctx.queueCall(engine.waitUntil, [false, false, key]);
                    break;
                case 2 /* timer */:
                    $ctx.queueCall(this.waitUntil, [engine.calculateDisplayLength(message), false, false]);
                    break;
                case 3 /* mouse or key */:
                    $ctx.queueCall(this.waitUntil, [false, true, key]);
                    break;
                case 4 /* mouse or timer */:
                    $ctx.queueCall(this.waitUntil, [engine.calculateDisplayLength(message), true, false]);
                    break;
            }
            $ctx.queueCall(function() {
                for (var i = 0; i < lines.length; i++) {
                    renderer.removeOverlay(lines[i]);
                }
            });
            return $ctx.finish();
        }),
        Character$$SayAt: util.blockingFunction(["chr", "x", "y", "width", "message"], function($ctx, $stk, $vars) {
            var chr = $vars["chr"], x = $vars["x"], y = $vars["y"], width = $vars["width"], message = $vars["message"];
            var engine = $ctx.engine;
            var game = engine.game.game;
            if (game.skipping_cutscene) {
                return $ctx.finish();
            }
            var renderer = engine.renderer;
            var font = $ctx.engine.game.fonts[3];
            var lines = font.splitLines(message, width);
            if (lines.length === 1 && lines[0] === '...') {
                lines[0] = '';
            }
            var totalWidth = 0;
            var totalHeight = font.def.lineHeight * lines.length;
            for (var i = 0; i < lines.length; i++) {
                var wid = font.stringWidth(lines[i]);
                lines[i] = {text:lines[i], width:wid};
                totalWidth = Math.max(totalWidth, wid);
            }
            y = Math.max(5, Math.min(215 - totalHeight, y - totalHeight));
            for (var i = 0; i < lines.length; i++) {
                lines[i] = renderer.createTextOverlay(
                    x,
                    y + font.def.lineHeight * i,
                    totalWidth,
                    3,
                    chr.def.speechColor,
                    lines[i].text);
            }
            var key = game.skip_speech_specific_key || true;
            switch(engine.game.skip_speech) {
                case 0 /* mouse, key or timer */:
                    $ctx.queueCall(engine.waitUntil, [engine.calculateDisplayLength(message), true, key]);
                    break;
                case 1 /* key or timer */:
                    $ctx.queueCall(engine.waitUntil, [false, false, key]);
                    break;
                case 2 /* timer */:
                    $ctx.queueCall(this.waitUntil, [engine.calculateDisplayLength(message), false, false]);
                    break;
                case 3 /* mouse or key */:
                    $ctx.queueCall(this.waitUntil, [false, true, key]);
                    break;
                case 4 /* mouse or timer */:
                    $ctx.queueCall(this.waitUntil, [engine.calculateDisplayLength(message), true, false]);
                    break;
            }
            $ctx.queueCall(function() {
                for (var i = 0; i < lines.length; i++) {
                    renderer.removeOverlay(lines[i]);
                }
            });
            return $ctx.finish();
        }),
        Character$$SayBackground: function(chr, message) { /* TODO */ },
        Character$$Think: function(chr, message, fmtArgs) {
            message = ags.util.format(message, fmtArgs);
            /* TODO */
        },
        Character$$get_Speaking: function(chr) { /* TODO */ return 0; },


        // Inventory Items
        Game$$get_InventoryItemCount:     function()          { return this.game.inventory.length; },
        InventoryItem$$get_ID:            function(item)      { return item.number;                },
        InventoryItem$$get_Name:          function(item)      { /* TODO */ return null;            },
        InventoryItem$$set_Name:          function(item,name) { /* TODO */                         },
        InventoryItem$$get_CursorGraphic: function(item)      { /* TODO */ return 0;               },
        InventoryItem$$set_CursorGraphic: function(item,img)  { /* TODO */                         },
        InventoryItem$$get_Graphic:       function(item)      { /* TODO */ return 0;               },
        InventoryItem$$set_Graphic:       function(item,img)  { /* TODO */                         },
        
        // Dialogs
        Game$$get_DialogCount:       function()           { /* TODO */ return 0;    },
        Dialog$$get_ID:              function(d)          { return d.number;        },
        Dialog$$get_OptionCount:     function(d)          { return d.options.length-1;    },
        Dialog$$DisplayOptions:      function(d,sayStyle) { /* TODO */ return 0;    },
        Dialog$$GetOptionState: function(d, opt) {
            return d.options[opt].state;
        },
        Dialog$$GetOptionText: function(d, opt) {
            return d.options[opt].text;
        },
        Dialog$$HasOptionBeenChosen: function(d, opt)     { /* TODO */ return 0;    },
        Dialog$$SetOptionState: function(d, opt, st) {
            d.options[opt].state = st;
        },
        inDialog: false,
        Dialog$$Start: function(dialog) {
            this.blockingThread.queueCall(this.runDialog, [dialog]);
        },
        runDialog: util.blockingFunction(["dialog"], function($ctx, $stk, $vars) {
            var dialog = $vars["dialog"];
            var dialogFunc = $ctx.engine.game.globalScripts.scripts["__DialogScripts.asc"]["_run_dialog" + dialog.number];
            var engine = $ctx.engine;
            switch($ctx.entryPoint) {
                case 0:
                    engine.inDialog = true;
                    return $ctx.nextEntryPoint(1);
                case 1:
                    $ctx.beginCall(dialogFunc, [0]);
                    return $ctx.nextEntryPoint(2);
                case 2:
                    if (!engine.inDialog) {
                        return $ctx.finish();
                    }
                    var ret = $stk.pop();
                    switch(ret) {
                        case -1 /* RUN_DIALOG_RETURN */:
                            return $ctx.nextEntryPoint(3);
                        case -2 /* RUN_DIALOG_STOP_DIALOG */:
                            engine.inDialog = false;
                            return $ctx.finish();
                        default:
                            $vars["dialog"] = engine.game.dialog[ret];
                            return $ctx.nextEntryPoint(1);
                    }
                case 3:
                    if (!engine.inDialog) {
                        return $ctx.finish();
                    }
                    var only_i = null;
                    for (var i = 1; i < dialog.options.length; i++) {
                        if (dialog.options[i].state === 1 /* eOptionOn */) {
                            if (only_i === null) {
                                only_i = i;
                            }
                            else {
                                only_i = false;
                                break;
                            }
                        }
                    }
                    if (only_i === null) {
                        engine.inDialog = false;
                        return $ctx.finish(); // no options at all!
                    }
                    else if (only_i !== false && !engine.game.game.show_single_dialog_option) {
                        // only one option
                        var onlyOption = dialog.options[only_i];
                        if (onlyOption.say) {
                            $ctx.queueCall(engine.Character$$Say, [engine.game.player, onlyOption.text]);
                        }
                        $ctx.beginCall(dialogFunc, [only_i]);
                        return $ctx.nextEntryPoint(2);
                    }
                    var dialogOptionGui = engine.game.gui[engine.game.def.settings.dialogOptionsGui];
                    var optionOverlays = [];
                    var selectedOverlay = null;
                    var mouse = engine.mouse;
                    var frameHandler = function(e) {
                        var i;
                        var y = mouse.nextY;
                        var newSelectedOverlay = null;
                        if (y >= optionOverlays.top) {
                            for (i = 0; i < optionOverlays.length; i++) {
                                if (y < optionOverlays[i].bottom) {
                                    newSelectedOverlay = optionOverlays[i];
                                    break;
                                }
                            }
                        }
                        if (newSelectedOverlay != selectedOverlay) {
                            if (selectedOverlay) {
                                for (i = 0; i < selectedOverlay.length; i++) {
                                    var line = selectedOverlay[i];
                                    engine.renderer.setOverlayText(
                                        line,
                                        line.x,
                                        line.y,
                                        line.width,
                                        line.font,
                                        engine.game.player.def.speechColor,
                                        line.text);
                                }
                            }
                            selectedOverlay = newSelectedOverlay;
                            if (selectedOverlay) {
                                for (i = 0; i < selectedOverlay.length; i++) {
                                    var line = selectedOverlay[i];
                                    engine.renderer.setOverlayText(
                                        line,
                                        line.x,
                                        line.y,
                                        line.width,
                                        line.font,
                                        65514,
                                        line.text);
                                }
                            }
                        }
                    };
                    $(engine).bind('frame', frameHandler);
                    $(engine).bind('mouseDown', function(e) {
                        if (selectedOverlay != null) {
                            $(engine).unbind('frame', frameHandler);
                            $(this).unbind(e);
                            $(engine).trigger('dialogOptionChosen', [dialog.options[selectedOverlay.number]]);
                        }
                    });
                    $(engine).bind('dialogOptionChosen', function(e, option) {
                        $(this).unbind(e);
                        for (var i = 0; i < optionOverlays.length; i++) {
                            for (var j = 0; j < optionOverlays[i].length; j++) {
                                engine.renderer.removeOverlay(optionOverlays[i][j]);
                            }
                        }
                        $("#dialog_list").empty();
                        if (dialogOptionGui) {
                            dialogOptionGui.setParam("visible", false);
                        }
                        $ctx.blocking = false;
                        if (option.say) {
                            $ctx.queueCall(engine.Character$$Say, [engine.game.player, option.text]);
                        }
                        $ctx.beginCall(dialogFunc, [option.number]);
                        $ctx.nextEntryPoint(2);
                    });
                    var optionX = engine.game.game.dialog_options_x;
                    var optionY = dialogOptionGui ? dialogOptionGui.def.top + 1 : 0;
                    var firstLineWidth = 320 - optionX - 15;
                    var subsequentLineWidth = firstLineWidth - indent;
                    var indent = 9;
                    var dialogOptionFont = engine.game.fonts[3];
                    optionOverlays.top = optionY;
                    for (var i = 1; i < dialog.options.length; i++) {
                        if (dialog.options[i].state === 1 /* eOptionOn */) {
                        
                            var lines = dialogOptionFont.splitLines(dialog.options[i].text, firstLineWidth, subsequentLineWidth);
                            lines.number = i;
                            optionOverlays.push(lines);
                            
                            for (var j = 0; j < lines.length; j++) {
                                if (j === 0) {
                                    lines[j] = engine.renderer.createTextOverlay(
                                        optionX, optionY, 320, 3, engine.game.player.def.speechColor, lines[j]);
                                }
                                else {
                                    lines[j] = engine.renderer.createTextOverlay(
                                        optionX + indent, optionY, 320, 3, engine.game.player.def.speechColor, lines[j]);
                                }
                                optionY += dialogOptionFont.def.lineHeight;
                            }
                            lines.bottom = optionY;
                        
                        /*
                            (function(option){
                            
                                $("<a>").attr("href", "#").text(option.text).appendTo("#dialog_list").wrap("<li>")
                                    .click(function(){
                                        $(engine).trigger('dialogOptionChosen', [option]);
                                    });
                                    
                            })(dialog.options[i]);
                            */
                        
                        }
                    }
                    engine.game.gui[engine.game.def.settings.dialogOptionsGui].setParam("visible", true);
                    $ctx.blocking = true;
                    return $ctx.finish();
            }
        }),

        /*** Scripting API: GUIs ***/
        
        // Top-Level GUIs
        FindGUIID:                  function(guiName) { return this.game.guiNumbersByName[guiName]; },
        Game$$get_GUICount:         function()        { return this.game.guis.length;               },
        GUI$$get_ID:                function(gui)     { return gui.number;                          },
        GUI$$get_ControlCount:      function(gui)     { return gui.controls.length;                 },
        GUI$$geti_Controls:         function(gui,i)   { return gui.controls[i];                     },
        GUI$$SetSize:               function(gui,w,h) { /* TODO */                                  },
        GUI$$get_Width:             function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_Width:             function(gui,w)   { /* TODO */                                  },
        GUI$$get_Height:            function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_Height:            function(gui,h)   { /* TODO */                                  },
        GUI$$SetPosition:           function(gui,x,y) { /* TODO */                                  },
        GUI$$get_X:                 function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_X:                 function(gui,x)   { /* TODO */                                  },
        GUI$$get_Y:                 function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_Y:                 function(gui,y)   { /* TODO */                                  },
        GUI$$Centre:                function(gui)     { /* TODO */                                  },
        GUI$$get_BackgroundGraphic: function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_BackgroundGraphic: function(gui,img) { /* TODO */                                  },
        GUI$$get_Clickable:         function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_Clickable:         function(gui,c)   { /* TODO */                                  },
        GUI$$get_Transparency:      function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_Transparency:      function(gui,t)   { /* TODO */                                  },
        GUI$$get_Visible:           function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_Visible:           function(gui,v)   { /* TODO */                                  },
        GUI$$get_ZOrder:            function(gui)     { /* TODO */ return 0;                        },
        GUI$$set_ZOrder:            function(gui,z)   { /* TODO */                                  },
        
        // Common GUI Control Methods
        GUIControl$$get_ID:          function(ctrl)     { return ctrl.number;     },
        GUIControl$$get_OwningGUI:   function(ctrl)     { return ctrl.gui;        },
        GUIControl$$get_AsButton:    function(ctrl)     { /* TODO */ return null; },
        GUIControl$$get_AsInvWindow: function(ctrl)     { /* TODO */ return null; },
        GUIControl$$get_AsLabel:     function(ctrl)     { /* TODO */ return null; },
        GUIControl$$get_AsListBox:   function(ctrl)     { /* TODO */ return null; },
        GUIControl$$get_AsSlider:    function(ctrl)     { /* TODO */ return null; },
        GUIControl$$get_AsTextBox:   function(ctrl)     { /* TODO */ return null; },
        GUIControl$$SetPosition:     function(ctrl,x,y) { /* TODO */              },
        GUIControl$$SetSize:         function(ctrl,w,h) { /* TODO */              },
        GUIControl$$get_X:           function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_X:           function(ctrl,x)   { /* TODO */              },
        GUIControl$$get_Y:           function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_Y:           function(ctrl,y)   { /* TODO */              },
        GUIControl$$get_Width:       function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_Width:       function(ctrl,w)   { /* TODO */              },
        GUIControl$$get_Height:      function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_Height:      function(ctrl,h)   { /* TODO */              },
        GUIControl$$BringToFront:    function(ctrl)     { /* TODO */              },
        GUIControl$$SendToBack:      function(ctrl)     { /* TODO */              },
        GUIControl$$get_Visible:     function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_Visible:     function(ctrl,v)   { /* TODO */              },
        GUIControl$$get_Enabled:     function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_Enabled:     function(ctrl,e)   { /* TODO */              },
        GUIControl$$get_Clickable:   function(ctrl)     { /* TODO */ return 0;    },
        GUIControl$$set_Clickable:   function(ctrl,c)   { /* TODO */              },
        
        // GUI Labels
        Label$$get_Text:      function(lbl)     { /* TODO */ return null; },
        Label$$set_Text:      function(lbl,txt) { /* TODO */              },
        Label$$get_TextColor: function(lbl)     { /* TODO */ return 0;    },
        Label$$set_TextColor: function(lbl,col) { /* TODO */              },
        Label$$get_Font:      function(lbl)     { /* TODO */ return 0;    },
        Label$$set_Font:      function(lbl,fnt) { /* TODO */              },

        // GUI TextBoxes
        TextBox$$get_Text:      function(tbox)     { /* TODO */ return null; },
        TextBox$$set_Text:      function(tbox,txt) { /* TODO */              },
        TextBox$$get_TextColor: function(tbox)     { /* TODO */ return 0;    },
        TextBox$$set_TextColor: function(tbox,col) { /* TODO */              },
        TextBox$$get_Font:      function(tbox)     { /* TODO */ return 0;    },
        TextBox$$set_Font:      function(tbox,fnt) { /* TODO */              },

        // GUI Buttons
        Button$$get_Text:             function(btn)      { /* TODO */ return 0; },
        Button$$set_Text:             function(btn,txt)  { /* TODO */           },
        Button$$get_TextColor:        function(btn)      { /* TODO */ return 0; },
        Button$$set_TextColor:        function(btn,col)  { /* TODO */           },
        Button$$get_Font:             function(btn)      { /* TODO */ return 0; },
        Button$$set_Font:             function(btn,fnt)  { /* TODO */           },
        Button$$get_NormalGraphic:    function(btn)      { /* TODO */ return 0; },
        Button$$set_NormalGraphic:    function(btn,slot) { /* TODO */           },
        Button$$get_PushedGraphic:    function(btn)      { /* TODO */ return 0; },
        Button$$set_PushedGraphic:    function(btn,slot) { /* TODO */           },
        Button$$get_MouseOverGraphic: function(btn)      { /* TODO */ return 0; },
        Button$$set_MouseOverGraphic: function(btn,slot) { /* TODO */           },
        Button$$get_ClipImage:        function(btn)      { /* TODO */ return 0; },
        Button$$set_ClipImage:        function(btn,clip) { /* TODO */           },
        Button$$get_Graphic:          function(btn)      { /* TODO */ return 0; },
        Button$$Animate: function(btn, view, loop, delay, repeatStyle) {
            /* TODO */
        },

        // GUI Sliders
        Slider$$get_BackgroundGraphic: function(sldr)      { /* TODO */ return 0; },
        Slider$$set_BackgroundGraphic: function(sldr,slot) { /* TODO */           },
        Slider$$get_HandleGraphic:     function(sldr)      { /* TODO */ return 0; },
        Slider$$set_HandleGraphic:     function(sldr,slot) { /* TODO */           },
        Slider$$get_HandleOffset:      function(sldr)      { /* TODO */ return 0; },
        Slider$$set_HandleOffset:      function(sldr,off)  { /* TODO */           },
        Slider$$get_Max:               function(sldr)      { /* TODO */ return 0; },
        Slider$$set_Max:               function(sldr,max)  { /* TODO */           },
        Slider$$get_Min:               function(sldr)      { /* TODO */ return 0; },
        Slider$$set_Min:               function(sldr,min)  { /* TODO */           },
        Slider$$get_Value:             function(sldr)      { /* TODO */ return 0; },
        Slider$$set_Value:             function(sldr,v)    { /* TODO */           },

        // GUI Inventory Windows
        UpdateInventory:               function()       { /* TODO */              },
        InvWindow$$get_ItemCount:      function(iw)     { /* TODO */ return 0;    },
        InvWindow$$geti_ItemAtIndex:   function(iw,i)   { /* TODO */ return null; },
        InvWindow$$ScrollDown:         function(iw)     { /* TODO */              },
        InvWindow$$ScrollUp:           function(iw)     { /* TODO */              },
        InvWindow$$get_CharacterToUse: function(iw)     { /* TODO */ return null; },
        InvWindow$$set_CharacterToUse: function(iw,chr) { /* TODO */              },
        InvWindow$$get_ItemWidth:      function(iw)     { /* TODO */ return 0;    },
        InvWindow$$set_ItemWidth:      function(iw,w)   { /* TODO */              },
        InvWindow$$get_ItemHeight:     function(iw)     { /* TODO */ return 0;    },
        InvWindow$$set_ItemHeight:     function(iw,h)   { /* TODO */              },
        InvWindow$$get_TopItem:        function(iw)     { /* TODO */ return 0;    },
        InvWindow$$set_TopItem:        function(iw,top) { /* TODO */              },
        InvWindow$$get_ItemsPerRow:    function(iw)     { /* TODO */ return 0;    },
        InvWindow$$get_RowCount:       function(iw)     { /* TODO */ return 0;    },

        // GUI List Boxes
        ListBox$$get_ItemCount:        function(lbox)       { /* TODO */ return 0;    },
        ListBox$$AddItem:              function(lbox,txt)   { /* TODO */ return 0;    },
        ListBox$$geti_Items:           function(lbox,i)     { /* TODO */ return null; },
        ListBox$$seti_Items:           function(lbox,i,txt) { /* TODO */              },
        ListBox$$InsertItemAt:         function(lbox,i,txt) { /* TODO */ return 0;    },
        ListBox$$RemoveItem:           function(lbox,i)     { /* TODO */              },
        ListBox$$Clear:                function(lbox)       { /* TODO */              },
        ListBox$$FillDirList:          function(lbox,mask)  { /* TODO */ return 0;    },
        ListBox$$FillSaveGameList:     function(lbox)       { /* TODO */ return 0;    },
        ListBox$$GetItemAtLocation:    function(lbox,x,y)   { /* TODO */ return 0;    },
        ListBox$$ScrollDown:           function(lbox)       { /* TODO */              },
        ListBox$$ScrollUp:             function(lbox)       { /* TODO */              },
        ListBox$$get_Font:             function(lbox)       { /* TODO */ return 0;    },
        ListBox$$set_Font:             function(lbox,fnt)   { /* TODO */              },
        ListBox$$get_HideBorder:       function(lbox)       { /* TODO */ return 0;    },
        ListBox$$set_HideBorder:       function(lbox,hide)  { /* TODO */              },
        ListBox$$get_HideScrollArrows: function(lbox)       { /* TODO */ return 0;    },
        ListBox$$set_HideScrollArrows: function(lbox,hide)  { /* TODO */              },
        ListBox$$get_RowCount:         function(lbox)       { /* TODO */ return 0;    },
        ListBox$$geti_SaveGameSlots:   function(lbox,i)     { /* TODO */ return 0;    },
        ListBox$$get_SelectedIndex:    function(lbox)       { /* TODO */ return 0;    },
        ListBox$$set_SelectedIndex:    function(lbox,i)     { /* TODO */              },
        ListBox$$get_TopItem:          function(lbox)       { /* TODO */ return 0;    },
        ListBox$$set_TopItem:          function(lbox,i)     { /* TODO */              },
        
        // Built-In GUIs
        RestoreGameDialog: function()       { /* TODO */              },
        SaveGameDialog:    function()       { /* TODO */              },
        Game$$InputBox:    function(prompt) { /* TODO */ return null; },
        
        // Message Display
        Display: function(message, fmtArgs) {
            message = ags.util.format(message, fmtArgs);
            /* TODO */
        },
        DisplayAt: function(x, y, width, message, fmtArgs) {
            message = ags.util.format(message, fmtArgs);
            /* TODO */
        },
        DisplayTopBar: function(y, textColor, backColor, title, message, fmtArgs) {
            message = ags.util.format(message, fmtArgs);
            /* TODO */
        },
        DisplayAtY:        function(y, message)   { /* TODO */ },
        DisplayMessage:    function(msgNumber)    { /* TODO */ },
        DisplayMessageAtY: function(msgNumber, y) { /* TODO */ },
        DisplayMessageBar: function(y, textCol, backCol, title, msgNumber) {
            /* TODO */
        },
        
        /*** Scripting API: Parser ***/
        
        Parser$$ParseText:       function(text)       { /* TODO */                      },
        Parser$$Said:            function(text)       { /* TODO */ return 0;            },
        Parser$$SaidUnknownWord: function()           { /* TODO */ return null;         },
        Parser$$FindWordID:      function(wordToFind) { /* TODO */ return 0;            },
        Said:                    function(text)       { return this.Parser$$Said(text); },
        // NOTE: Old-style Said() does belong here, not in compat
        
        // Misc. Utilities
        Debug: function(command,data) { },
        RunAGSGame: function(filename, mode, data) { },
        Game$$DoOnceOnly: function(token) {
            var alreadyDone = this.game.onceOnly.hasOwnProperty(token);
            this.game.onceOnly[token] = true;
            return alreadyDone ? 0 : 1;
        },

        // Pausing and Disabling Interface
        PauseGame: function() { /* TODO */ },
        UnPauseGame: function() { /* TODO */ },
        IsGamePaused: function() { /* TODO */ return 0; },
        DisableInterface: function() { this.interfaceEnabled = false; },
        EnableInterface: function() { this.interfaceEnabled = true; },
        IsInterfaceEnabled: function() { return this.interfaceEnabled | 0; },
        
        // Mouse Cursor Modes and Visibility
        Game$$get_MouseCursorCount:     function()           { return this.game.mouseCursors.length; },
        Mouse$$get_Mode:                function()           { /* TODO */ return 0;                  },
        Mouse$$set_Mode:                function(mode)       { /* TODO */                            },
        Mouse$$ChangeModeGraphic:       function(mode, slot) { /* TODO */                            },
        Mouse$$ChangeModeHotspot:       function(mode, x, y) { /* TODO */                            },
        Mouse$$ChangeModeView:          function(mode, view) { /* TODO */                            },
        Mouse$$EnableMode:              function(mode)       { /* TODO */                            },
        Mouse$$DisableMode:             function(mode)       { /* TODO */                            },
        Mouse$$GetModeGraphic:          function(mode)       { /* TODO */ return 0;                  },
        Mouse$$SaveCursorUntilItLeaves: function()           { /* TODO */                            },
        Mouse$$SelectNextMode:          function()           { /* TODO */                            },
        Mouse$$UseDefaultGraphic:       function()           { /* TODO */                            },
        Mouse$$UseModeGraphic:          function(mode)       { /* TODO */                            },
        Mouse$$get_Visible:             function()           { /* TODO */ return 0;                  },
        Mouse$$set_Visible:             function(v)          { /* TODO */                            },
        
        // Interaction
        IsInteractionAvailable:                function(x, y, mode) { /* TODO */ return 0; },
        InventoryItem$$IsInteractionAvailable: function(item, mode) { /* TODO */ return 0; },
        ProcessClick: util.blockingFunction(["x","y","mode"], function($ctx, $stk, $vars) {
            var x = $vars["x"], y = $vars["y"], mode = $vars["mode"];
            // TODO: use the x and y params instead of ignoring them completely
            var mouseOver = $ctx.engine.mouseOver;
            mode = 2;
            var handler = mouseOver && mouseOver.def && mouseOver.def.clickModeHandlers && mouseOver.def.clickModeHandlers[mode];
            if (handler) {
                if (typeof handler.script === 'undefined') {
                    $ctx.queueCall($ctx.engine.room.script.script[handler.func]);
                }
                else {
                    $ctx.queueCall($ctx.engine.game.globalScripts.scripts[handler.script][handler.func]);
                }
            }
            return $ctx.finish();
        }),
        Character$$RunInteraction:             function(mode)       { /* TODO */           },
        InventoryItem$$RunInteraction:         function(mode)       { /* TODO */           },
        Hotspot$$RunInteraction:               function(mode)       { /* TODO */           },
        Object$$RunInteraction:                function(mode)       { /* TODO */           },
        Region$$RunInteraction:                function(event)      { /* TODO */           },

        // Custom Properties
        Character$$GetProperty:         function(chr,name)  { /* TODO */ return 0;    },
        Character$$GetTextProperty:     function(chr,name)  { /* TODO */ return null; },
        InventoryItem$$GetProperty:     function(item,name) { /* TODO */ return 0;    },
        InventoryItem$$GetTextProperty: function(item,name) { /* TODO */ return null; },
        GetRoomProperty:                function(name)      { /* TODO */ return 0;    },
        Room$$GetTextProperty:          function(name)      { /* TODO */ return null; },
        Hotspot$$GetProperty:           function(hs,name)   { /* TODO */ return 0;    },
        Hotspot$$GetTextProperty:       function(hs,name)   { /* TODO */ return null; },
        Object$$GetProperty:            function(obj,name)  { /* TODO */ return 0;    },
        Object$$GetTextProperty:        function(obj,name)  { /* TODO */ return null; },
        
        /*** Scripting API: Room-Specific ***/
        
        // Room
        GetBackgroundFrame:                   function()  { /* TODO */ return 0;              },
        SetBackgroundFrame:                   function(i) { this.renderer.setBackground(this.room.backgrounds[i]); },
        Room$$GetDrawingSurfaceForBackground: function(i) { /* TODO */ return null;           },
        Room$$get_BottomEdge:                 function()  { return this.room.def.bottomEdge;  },
        Room$$get_ColorDepth:                 function()  { return this.room.def.colorDepth;  },
        Room$$get_Height:                     function()  { return this.room.def.height;      },
        Room$$get_LeftEdge:                   function()  { return this.room.def.leftEdge;    },
        Room$$geti_Messages:                  function(i) { /* TODO */ return null;           },
        Room$$get_MusicOnLoad:                function()  { return this.room.def.musicOnLoad; },
        Room$$get_ObjectCount:                function()  { return this.room.objects.count;   },
        Room$$get_RightEdge:                  function()  { return this.room.def.rightEdge;   },
        Room$$get_TopEdge:                    function()  { return this.room.def.topEdge;     },
        Room$$get_Width:                      function()  { return this.room.def.width;       },
        CallRoomScript:                       function(value) { },
        GetScalingAt:                         function(x,y) { /* TODO */ return 0; },
        Region$$GetAtRoomXY:                  function(x,y) { /* TODO */ return null; },
        
        // Walkable Areas
        RemoveWalkableArea: function(area) { /* TODO */ },
        RestoreWalkableArea: function(area) { /* TODO */ },
        SetAreaScaling: function(area, min, max) { /* TODO */ },
        DisableGroundLevelAreas: function(disableTints) { /* TODO */ },
        EnableGroundLevelAreas: function() { /* TODO */ },
        
        // Walk-Behinds
        SetWalkBehindBase: function(wb, baseline) {
            /* TODO */
            this.room.walkbehinds[wb].baseline = baseline;
            this.renderer.setWalkbehindBase(wb, baseline);
        },
        
        // Scene Viewport
        System$$get_ViewportWidth:    function()      { /* TODO */ return 0;    },
        System$$get_ViewportHeight:   function()      { /* TODO */ return 0;    },
        GetViewportX:                 function()      { /* TODO */ return 0;    },
        GetViewportY:                 function()      { /* TODO */ return 0;    },
        SetViewport:                  function(rx,ry) { /* TODO */              },
        ReleaseViewport:              function()      { /* TODO */              },
        GetLocationType:              function(x,y)   { /* TODO */ return 0;    },
        Game$$GetLocationName:        function(x,y)   { /* TODO */ return null; },
        GUI$$GetAtScreenXY:           function(x,y)   { /* TODO */ return null; },
        Character$$GetAtScreenXY:     function(x,y)   { /* TODO */ return null; },
        Hotspot$$GetAtScreenXY:       function(x,y)   { /* TODO */ return null; },
        InventoryItem$$GetAtScreenXY: function(x,y)   { /* TODO */ return null; },
        GUIControl$$GetAtScreenXY:    function(x,y)   { /* TODO */ return null; },
        Object$$GetAtScreenXY:        function(x,y)   { /* TODO */ return null; },
        GetWalkableAreaAt:            function(x,y)   { /* TODO */ return 0;    },
        
        // Room Objects
        Object$$get_ID: function(obj) { return obj.number; },
        Object$$Animate: util.blockingFunction(
            ["obj","loop","delay","repeatStyle","blockStyle","direction"],
            function($ctx, $stk, $vars) {
                var obj = $vars["obj"], loop = $vars["loop"], delay = $vars["delay"], repeatStyle = $vars["repeatStyle"],
                    blockStyle = $vars["blockStyle"], direction = $vars["direction"];
                $ctx.queueCall($ctx.engine.animateSprite, [obj, 0, loop, delay, repeatStyle, blockStyle, direction]);
                return $ctx.finish();
            }
        ),
        Object$$IsCollidingWithObject: function(obj, other) { /* TODO */ return 0; },
        Object$$MergeIntoBackground: function(obj) { /* TODO */ return 0; },
        Object$$Move: util.blockingFunction(["obj","x","y","speed","blockStyle","walkWhere"], function($ctx, $stk, $vars) {
            var obj = $vars["obj"], x = $vars["x"], y = $vars["y"], speed = $vars["speed"], blockStyle = $vars["blockStyle"], walkWhere = $vars["walkWhere"];
            var engine = $ctx.engine;
            if (obj.sprite) {
                $ctx.queueCall(engine.spriteMove, [obj.sprite, x, y, speed, blockStyle, walkWhere, false]);
            }
            return $ctx.finish(0);
        }),
        Object$$RemoveTint: function(obj) { /* TODO */ },
        Object$$SetPosition: function(obj,x,y) { /* TODO */ },
        Object$$SetView: function(obj,view,loop,frame) { /* TODO */ },
        Object$$StopAnimating: function(obj) { /* TODO */ return 0; },
        Object$$StopMoving: function() { /* TODO */ return 0; },
        Object$$Tint: function(r,g,b,sat,lum) { /* TODO */ },
        Object$$get_Animating: function(obj) { /* TODO */ return 0; },
        Object$$get_Baseline: function(obj) { /* TODO */ return 0; },
        Object$$set_Baseline: function(obj,bl) {
            /* TODO */
            obj.sprite.setParam("baseline", bl);
        },
        Object$$get_BlockingHeight: function(obj) { /* TODO */ return 0; },
        Object$$set_BlockingHeight: function(obj,h) { /* TODO */ },
        Object$$get_BlockingWidth: function(obj) { /* TODO */ return 0; },
        Object$$set_BlockingWidth: function(obj,w) { /* TODO */ },
        Object$$get_Clickable: function(obj) { /* TODO */ return 0; },
        Object$$set_Clickable: function(obj,clickable) { /* TODO */ },
        Object$$get_Frame: function(obj) { /* TODO */ return 0; },
        Object$$get_Graphic: function(obj) {
            /* TODO */
            return obj.sprite.getParam("image");
        },
        Object$$set_Graphic: function(obj,slot) {
            /* TODO */
            obj.sprite.setParam("image", slot);
        },
        Object$$get_IgnoreScaling: function(obj) { /* TODO */ return 0; },
        Object$$set_IgnoreScaling: function(obj,ignore) { /* TODO */ },
        Object$$get_IgnoreWalkbehinds: function(obj) { /* TODO */ return 0; },
        Object$$set_IgnoreWalkbehinds: function(obj,ignore) { /* TODO */ },
        Object$$get_Loop: function(obj) { /* TODO */ return 0; },
        Object$$get_Moving: function(obj) { /* TODO */ return 0; },
        Object$$get_Name: function(obj) { /* TODO */ return null; },
        Object$$get_Solid: function(obj) { /* TODO */ return 0; },
        Object$$set_Solid: function(obj,solid) { /* TODO */ },
        Object$$get_Transparency: function(obj) { /* TODO */ return 0; },
        Object$$set_Transparency: function(obj,trans) { /* TODO */ return 0; },
        Object$$get_View: function(obj) { /* TODO */ return 0; },
        Object$$get_Visible: function(obj) {
            return obj.sprite.getParam("visible") | 0;
        },
        Object$$set_Visible:function(obj, vis) {
            obj.sprite.setParam("visible", !!vis);
        },
        Object$$get_X: function(obj) { /* TODO */ return obj.sprite.getParam("x"); },
        Object$$set_X: function(obj,x) { /* TODO */ },
        Object$$get_Y: function(obj) { /* TODO */ return 0; },
        Object$$set_Y: function(obj,x) { /* TODO */ },

        // Hotspots
        Hotspot$$get_ID: function(hs) { return hs.number; },
        Hotspot$$get_Name: function(hs) { /* TODO */ return null; },
        Hotspot$$get_Enabled: function(hs) { /* TODO */ return 0; },
        Hotspot$$set_Enabled: function(hs,enable) { /* TODO */ },
        Hotspot$$get_WalkToX: function(hs) { /* TODO */ return 0; },
        Hotspot$$get_WalkToY: function(hs) { /* TODO */ return 0; },
        
        // Regions
        Region$$get_ID: function(region) { return region.number; },
        Region$$get_Enabled: function(region) { /* TODO */ return 0; },
        Region$$set_Enabled: function(region,enabled) { /* TODO */ },
        Region$$get_LightLevel: function(region) { /* TODO */ return 0; },
        Region$$set_LightLevel: function(region,level) { /* TODO */ },
        Region$$Tint: function(region,r,g,b,amount) { /* TODO */ },
        Region$$get_TintEnabled: function(region) { /* TODO */ return 0; },
        Region$$get_TintBlue: function(region) { /* TODO */ return 0; },
        Region$$get_TintGreen: function(region) { /* TODO */ return 0; },
        Region$$get_TintRed: function(region) { /* TODO */ return 0; },
        Region$$get_TintSaturation: function(region) { /* TODO */ return 0; },
        
        // Video and Visual Effects
        System$$get_Gamma: function() { /* TODO */ return 0; },
        System$$set_Gamma: function(v) { /* TODO */ },
        getColorRgbString: function(agsColor) {
            var a = this.getColor(agsColor);
            return 'rgb(' + (a >> 16) + ',' + ((a >> 8) & 0xff) + ',' + (a & 0xff) + ')';
        },
        getColor: function(agsColor) {
            if (agsColor < 32) {
                switch(agsColor) {
                    case -1: return 0x000000; // COLOR_TRANSPARENT
                    case 0: return 0x000000;
                    case 1: return 0x0000A5;
                    case 2: return 0x00A500;
                    case 3: return 0x00A5A5;
                    case 4: return 0xA50000;
                    case 5: return 0xA500A5;
                    case 6: return 0xA5A500;
                    case 7: return 0xA5A5A5;
                    case 8: return 0x525252;
                    case 9: return 0x5252FF;
                    case 10: return 0x52FF52;
                    case 11: return 0x52FFFF;
                    case 12: return 0xFF5252;
                    case 13: return 0xFF52FF;
                    case 14: return 0xFFFF52;
                    case 15: return 0xFFFFFF;
                    case 16: return 0x000000;
                    case 17: return 0x101010;
                    case 18: return 0x212121;
                    case 19: return 0x313131;
                    case 20: return 0x424242;
                    case 21: return 0x525252;
                    case 22: return 0x636363;
                    case 23: return 0x737373;
                    case 24: return 0x848484;
                    case 25: return 0x949494;
                    case 26: return 0xA5A5A5;
                    case 27: return 0xB5B5B5;
                    case 28: return 0xC6C6C6;
                    case 29: return 0xD6D6D6;
                    case 30: return 0xE7E7E7;
                    case 31: return 0xF7F7F7;
                }
            }
            // be careful refactoring this into a single return statement...
            var returnValue = 0;
            returnValue |= ((agsColor >>> 11) & 0x1F) << 19; // red (top 5 bits)
            returnValue |= ((agsColor >>> 13) & 0x07) << 16; // red (bottom 3 bits)
            returnValue |= ((agsColor >>> 6) & 0x1F) << 11; // green (top 5 bits)
            returnValue |= ((agsColor >>> 8) & 0x07) << 8; // green (bottom 3 bits)
            returnValue |= (agsColor & 0x1F) << 3; // blue (top 5 bits)
            returnValue |= (agsColor >>> 2) & 0x07; // blue (bottom 3 bits)
            return returnValue;
        },
        Game$$GetColorFromRGB: function(r,g,b) { /* TODO */ return 0; },
        FlipScreen: function(way) { /* TODO */ },
        FadeIn: util.blockingFunction(['speed'], function($ctx, $stk, $vars) {
            var speed = $vars["speed"];
            var engine = $ctx.engine;
            var game = engine.game.game;
            var renderer = engine.renderer;
            $(renderer).bind('fadedIn', function(e) {
                $ctx.blocking = false;
                $(this).unbind(e);
            });
            $ctx.blocking = true;
            renderer.fadeIn(game.fade_color_red, game.fade_color_green, game.fade_color_blue, 64/speed);
            return $ctx.finish();
        }),
        FadeOut: util.blockingFunction(['speed'], function($ctx, $stk, $vars) {
            var speed = $vars["speed"];
            var engine = $ctx.engine;
            var game = engine.game.game;
            var renderer = engine.renderer;
            $(renderer).bind('fadedOut', function(e) {
                $ctx.blocking = false;
                $(this).unbind(e);
            });
            $ctx.blocking = true;
            renderer.fadeOut(game.fade_color_red, game.fade_color_green, game.fade_color_blue, 64/speed);
            return $ctx.finish();
        }),
        CyclePalette: function(start, end) { /* TODO */ },
        SetPalRGB: function(slot, r, g, b) { /* TODO */ },
        UpdatePalette: function() { /* TODO */ },
        TintScreen: function(r, g, b) { /* TODO */ },
        SetAmbientTint: function(r, g, b, sat, lum) { /* TODO */ },
        ShakeScreen: function(amount) { /* TODO */ },
        ShakeScreenBackground: function(delay, amount, length) { /* TODO */ },
        SetScreenTransition: function(style) { /* TODO */ },
        SetNextScreenTransition: function(style) { /* TODO */ },
        SetFadeColor: function(r,g,b) {
            this.game.game.fade_color_red = r;
            this.game.game.fade_color_green = g;
            this.game.game.fade_color_blue = b;
            this.renderer.setFadeColor(r,g,b);
        },
        PlayFlic: function(flcNumber, options) { /* TODO */ },
        PlayVideo: function(filename, skipStyle, flags) { /* TODO */ },

        /*** Script Interface: Audio ***/
        
        CDAudio:              function(cmd, data) { /* TODO */ return 0; },
        System$$get_Volume:   function()          { /* TODO */ return 0; },
        System$$set_Volume:   function(v)         { /* TODO */           },
        Game$$GetMODPattern:  function()          { /* TODO */ return 0; },
        Game$$IsAudioPlaying: function(audioType) { /* TODO */ return 0; },
        Game$$StopAudio:      function(audioType) { /* TODO */           },
        IsVoxAvailable: function() { /* TODO */ return 0; },
        SetSpeechVolume: function(volume) { /* TODO */ },
        IsMusicVoxAvailable: function() { /* TODO */ return 0; },
        
        AudioClip$$Play: function(clip, priority, repeatStyle) {
            if (priority === 31998 /* SCR_NO_VALUE */) {
            }
            if (repeatStyle === 31998 /* SCR_NO_VALUE */) {
            }
            $(this).trigger('playAudioClip', [clip]);
            /* TODO */
            return null;
        },
        AudioClip$$PlayQueued: function(clip, priority, repeatStyle) {
            if (priority === 31998 /* SCR_NO_VALUE */) {
            }
            if (repeatStyle === 31998 /* SCR_NO_VALUE */) {
            }
            /* TODO */
            return null;
        },
        AudioClip$$Stop:            function(clip) { /* TODO */           },
        AudioClip$$get_FileType:    function(clip) { /* TODO */ return 0; },
        AudioClip$$get_IsAvailable: function(clip) { /* TODO */ return 0; },
        AudioClip$$get_Type:        function(clip) { /* TODO */ return 0; },

        System$$get_AudioChannelCount: function()        { /* TODO */ return 0;    },
        System$$geti_AudioChannels:    function(i)       { /* TODO */ return null; },
        AudioChannel$$get_ID:          function(chn)     { return chn.number;      },
        AudioChannel$$Seek:            function(chn,pos) { /* TODO */              },
        AudioChannel$$SetRoomLocation: function(chn,x,y) { /* TODO */              },
        AudioChannel$$Stop:            function(chn)     { /* TODO */              },
        AudioChannel$$get_IsPlaying:   function(chn)     { /* TODO */ return 0;    },
        AudioChannel$$get_LengthMs:    function(chn)     { /* TODO */ return 0;    },
        AudioChannel$$get_Panning:     function(chn)     { /* TODO */ return 0;    },
        AudioChannel$$set_Panning:     function(chn,pan) { /* TODO */              },
        AudioChannel$$get_PlayingClip: function(chn)     { /* TODO */ return null; },
        AudioChannel$$get_Position:    function(chn)     { /* TODO */ return 0;    },
        AudioChannel$$get_PositionMs:  function(chn)     { /* TODO */ return 0;    },
        AudioChannel$$get_Volume:      function(chn)     { /* TODO */ return 0;    },
        AudioChannel$$set_Volume:      function(chn,vol) { /* TODO */              },

        /*** Dynamic Graphics ***/

        // Overlays
        Overlay$$CreateGraphical: function(x,y,slot,transparent) { /* TODO */ return null; },
        Overlay$$CreateTextual: function(x,y,width,fontType,color,text,fmtArgs) {
            text = ags.util.format(text, fmtArgs);
            /* TODO */
        },
        Overlay$$SetText: function(ol, width, fontType, color, text, fmtArgs) {
            text = ags.util.format(text, fmtArgs);
            /* TODO */
        },
        Overlay$$Remove:    function(ol)   { /* TODO */           },
        Overlay$$get_Valid: function(ol)   { /* TODO */ return 0; },
        Overlay$$get_X:     function(ol)   { /* TODO */ return 0; },
        Overlay$$set_X:     function(ol,x) { /* TODO */           },
        Overlay$$get_Y:     function(ol)   { /* TODO */ return 0; },
        Overlay$$set_Y:     function(ol,y) { /* TODO */           },
        
        SaveScreenshot: function(filename) { /* TODO */ return 0; },

        DynamicSprite$$Create:                   function(w,h,alpha)        { /* TODO */ return null; },
        DynamicSprite$$CreateFromBackground:     function(i,x,y,w,h)        { /* TODO */ return null; },
        DynamicSprite$$CreateFromExistingSprite: function(slot,keepAlpha)   { /* TODO */ return null; },
        DynamicSprite$$CreateFromFile:           function(filename)         { /* TODO */ return null; },
        DynamicSprite$$CreateFromSaveGame:       function(slot,w,h)         { /* TODO */ return null; },
        DynamicSprite$$CreateFromScreenShot:     function(w,h)              { /* TODO */ return null; },
        DynamicSprite$$ChangeCanvasSize:         function(ds,w,h,x,y)       { /* TODO */              },
        DynamicSprite$$CopyTransparencyMask:     function(ds,slot)          { /* TODO */              },
        DynamicSprite$$Crop:                     function(ds,x,y,w,h)       { /* TODO */              },
        DynamicSprite$$Delete:                   function(ds)               { /* TODO */              },
        DynamicSprite$$Flip:                     function(ds,direction)     { /* TODO */              },
        DynamicSprite$$GetDrawingSurface:        function(ds)               { /* TODO */ return null; },
        DynamicSprite$$Resize:                   function(ds,w,h)           { /* TODO */              },
        DynamicSprite$$Rotate:                   function(ds,ang,w,h)       { /* TODO */              },
        DynamicSprite$$SaveToFile:               function(ds,filename)      { /* TODO */ return 0;    },
        DynamicSprite$$Tint:                     function(ds,r,g,b,sat,lum) { /* TODO */              },
        DynamicSprite$$get_ColorDepth:           function(ds)               { /* TODO */ return 0;    },
        DynamicSprite$$get_Graphic:              function(ds)               { /* TODO */ return 0;    },
        DynamicSprite$$get_Height:               function(ds)               { /* TODO */ return 0;    },
        DynamicSprite$$get_Width:                function(ds)               { /* TODO */ return 0;    },

        // Drawing Surface
        DrawingSurface$$get_Width:                  function(surf) { },
        DrawingSurface$$get_Height:                 function(surf) { },
        DrawingSurface$$get_UseHighResCoordinates:  function(surf) { },
        DrawingSurface$$set_UseHighResCoordinates:  function(surf, use) { },
        DrawingSurface$$get_DrawingColor:           function(surf) { },
        DrawingSurface$$set_DrawingColor:           function(surf, color) { },
        DrawingSurface$$Clear:                      function(surf, color) { },
        DrawingSurface$$DrawCircle:                 function(surf, x, y, radius) { },
        DrawingSurface$$DrawImage:                  function(surf, x, y, spriteSlot, transparency, width, height) { },
        DrawingSurface$$DrawLine:                   function(surf, x1, y1, x2, y2, thickness) { },
        DrawingSurface$$DrawMessageWrapped:         function(surf, x, y, width, fontType, messageNumber) { },
        DrawingSurface$$DrawPixel:                  function(surf, x, y) { },
        DrawingSurface$$DrawRectangle:              function(surf, x1, y1, x2, y2) { },
        DrawingSurface$$DrawString:                 function(surf, x, y, fontType, text, fmtArgs) { },
        DrawingSurface$$DrawStringWrapped:          function(surf, x, y, width, fontType, alignment, text) { },
        DrawingSurface$$DrawSurface:                function(surf, surfToDraw, transparency) { },
        DrawingSurface$$DrawTriangle:               function(surf, x1, y1, x2, y2, x3, y3) { },
        DrawingSurface$$GetPixel:                   function(surf, x, y) { },
        DrawingSurface$$CreateCopy:                 function(surf) { },
        DrawingSurface$$Release:                    function(surf) { },
        
        // Custom Dialog Rendering
        DialogOptionsRenderingInfo$$get_DialogToRender:     function(dop)     { return dop.dialog;       },
        DialogOptionsRenderingInfo$$get_Surface:            function(dop)     { return dop.surface;      },
        DialogOptionsRenderingInfo$$get_ActiveOptionID:     function(dop)     { return dop.activeOption; },
        DialogOptionsRenderingInfo$$set_ActiveOptionID:     function(dop,opt) { dop.activeOption = opt;  },
        DialogOptionsRenderingInfo$$get_X:                  function(dop)     { return dop.x;            },
        DialogOptionsRenderingInfo$$set_X:                  function(dop,x)   { dop.x = x;               },
        DialogOptionsRenderingInfo$$get_Y:                  function(dop)     { return dop.y;            },
        DialogOptionsRenderingInfo$$set_Y:                  function(dop,y)   { dop.y = y;               },
        DialogOptionsRenderingInfo$$get_Width:              function(dop)     { return dop.width;        },
        DialogOptionsRenderingInfo$$set_Width:              function(dop,w)   { dop.width = w;           },
        DialogOptionsRenderingInfo$$get_Height:             function(dop)     { return dop.height;       },
        DialogOptionsRenderingInfo$$set_Height:             function(dop,h)   { dop.height = h;          },
        DialogOptionsRenderingInfo$$get_ParserTextBoxX:     function(dop)     { return dop.parserX;      },
        DialogOptionsRenderingInfo$$set_ParserTextBoxX:     function(dop,x)   { dop.parserX = x;         },
        DialogOptionsRenderingInfo$$get_ParserTextBoxY:     function(dop)     { return dop.parserY;      },
        DialogOptionsRenderingInfo$$set_ParserTextBoxY:     function(dop,y)   { dop.parserY = y;         },
        DialogOptionsRenderingInfo$$get_ParserTextBoxWidth: function(dop)     { return dop.parserWidth;  },
        DialogOptionsRenderingInfo$$set_ParserTextBoxWidth: function(dop,w)   { dop.parserWidth = w;     }
        
    };
    
    ags.Room = function(engine, roomDef, number) {
        var room = this;
        this.number = number;
        this.def = roomDef;
        this.engine = engine;
        var game = engine.game;
        
        var objects = [];
        var hotspots = [];
        game.object = objects;
        game.hotspot = hotspots;
        game.region = [];
        
        var backgrounds = new Array(roomDef.numBackgrounds);
        this.backgrounds = backgrounds;
        
        var walkbehinds = new Array(roomDef.walkbehinds.length);
        this.walkbehinds = walkbehinds;
        
        var thread = engine.roomThread;
        this.thread = thread;
        
        var roomChecklist = new ags.Checklist("room");
        $(roomChecklist).bind('allCheckedOff', function(e) {
            for (var i = 0; i < roomDef.hotspots.length; i++) {
                var hotspotDef = roomDef.hotspots[i];
                var newHotspot = new ags.RoomHotspot(this, hotspotDef, i);
                hotspots[i] = newHotspot;
                game[hotspotDef.scritpName] = newHotspot;
            }
            
            engine.renderer.clearScene();
            
            for (var i = 0; i < roomDef.objects.length; i++) {
                var objectDef = roomDef.objects[i];
                var newObject = new ags.RoomObject(this, objectDef, i);
                objects[i] = newObject;
                game[objectDef.scriptName] = newObject;
                var objSprite = engine.renderer.createSprite();
                objSprite.represents = newObject;
                objSprite.setParams({
                    x: newObject.x,
                    y: newObject.y,
                    clickable: true,
                    visible: newObject.visible,
                    baseline: objectDef.baseline,
                    image: objectDef.image,
                    multY: -1
                });
                engine.renderer.addSpriteToLayer(objSprite, 'scene');
                newObject.sprite = objSprite;
            }
            
            for (var i = 0; i < game.character.length; i++) {
                if (game.character[i].room === number) {
                    var character = game.character[i];
                    character.deliverSprite();
                }
            }
            
            engine.renderer.setBackground(backgrounds[0]);
            engine.renderer.setWalkbehinds(walkbehinds);
            if (room.script.enterRoomBeforeFadeIn) {
                thread.queueCall(room.script.enterRoomBeforeFadeIn);
            }
            thread.queueCall(engine.Wait, [10]);
            thread.queueCall(engine.FadeIn, [2]);
            if (room.script.enterRoomAfterFadeIn) {
                thread.queueCall(room.script.enterRoomAfterFadeIn);
            }
            if (room.script.firstTimeEntersRoom) {
                thread.queueCall(room.script.firstTimeEntersRoom);
            }
            engine.renderer.startFrames();
        });
        
        for (var i = 0; i < roomDef.objects.length; i++) {
            var objectDef = roomDef.objects[i];
            engine.renderer.addNumberedImageToChecklist(roomChecklist, objectDef.image);
        }
        
        for (var i = 0; i < game.character.length; i++) {
            if (game.character[i].room === number) {
                game.character[i].addToChecklist(roomChecklist);
            }
        }
        
        var gameClasses = ags.games[game.def.guid];
        if (typeof gameClasses["Room" + roomDef.number + "Script"] !== 'undefined') {
            var RoomScriptClass = gameClasses["Room" + roomDef.number + "Script"];
            this.script = new RoomScriptClass(engine);
        }
        else {
            roomChecklist.add("script");
            $.ajax({
                url: game.getResourceLocation("roomScript", {roomNumber:roomDef.number}),
                dataType: "script",
                cache: false,
                success: function() {
                    var RoomScriptClass = gameClasses["Room" + roomDef.number + "Script"];
                    room.script = new RoomScriptClass(engine);
                    roomChecklist.checkOff("script");
                }
            });
        }
        
        roomChecklist.add("hotspotMask");
        engine.renderer.loadImage(game.getResourceLocation("roomHotspotMask", {roomNumber:roomDef.number}), function(img) {
            engine.renderer.setHotspotMask(img);
            roomChecklist.checkOff("hotspotMask");
        }, roomDef.width, roomDef.height);
        
        for (var i = 0; i < roomDef.numBackgrounds; i++) {
            roomChecklist.add("background");
            (function(){
                var num = i;
                var bgUrl = game.getResourceLocation("roomBackgroundImage", {roomNumber:roomDef.number, backgroundNumber:num});
                engine.renderer.loadImage(bgUrl, function(img) {
                    backgrounds[num] = img;
                    roomChecklist.checkOff("background");
                }, roomDef.width, roomDef.height);
            })();
        }
        
        for (var i = 1; i < roomDef.walkbehinds.length; i++) {
            roomChecklist.add("walkbehind");
            (function(){
                var num = i;
                var wbUrl = game.getResourceLocation("roomWalkbehindMask", {roomNumber:roomDef.number, walkbehindNumber:num});
                engine.renderer.loadImage(wbUrl, function(img) {
                    walkbehinds[num] = {image:img, baseline:roomDef.walkbehinds[num].baseline};
                    roomChecklist.checkOff("walkbehind");
                }, roomDef.width, roomDef.height);
            })();
        }
        
        roomChecklist.done();
    };
    ags.Room.prototype = {
        save: function() {
            var data = {};
            return data;
        }
    };
    
    ags.RoomObject = function(room, objectDef, number) {
        this.number = number;
        this.visible = objectDef.visible;
        this.def = objectDef;
        this.x = objectDef.x;
        this.y = objectDef.y
    };
    ags.RoomObject.prototype = {
        number: -1
    };

    ags.RoomHotspot = function(room, hotspotDef, number) {
        this.number = number;
        this.def = hotspotDef;
    };
    ags.RoomHotspot.prototype = {
        number: -1
    };

    ags.Game = function(engine, gameDef) {
        this.engine = engine;
        engine.game = this;
        this.def = gameDef;
        
        // storage for Game.DoOnceOnly
        this.onceOnly = {};
        
        // skip speech mode
        var skipSpeech = gameDef.settings.skipSpeech;
        if (skipSpeech.mouse) {
            if (skipSpeech.keyboard) {
                if (skipSpeech.timer) {
                    this.skip_speech = 0; // mouse, key or timer
                }
                else {
                    this.skip_speech = 3; // mouse or key
                }
            }
            else {
                this.skip_speech = 4; // mouse or timer
            }
        }
        else {
            if (skipSpeech.keyboard) {
                this.skip_speech = 1; // key or timer
            }
            else {
                this.skip_speech = 2; // timer only
            }
        }
        
        // animation
        var views = new Array(gameDef.views.length);
        views[0] = null; // 0th view never exists
        this.views = views;
        for (var i = 1; i < gameDef.views.length; i++) {
            views[i] = new ags.AnimationView(engine, gameDef.views[i], i);
        }
        
        // fonts
        var fonts = new Array(gameDef.fonts.length);
        this.fonts = fonts;
        for (var i = 0; i < gameDef.fonts.length; i++) {
            var font = gameDef.fonts[i];
            if (!font) {
                continue;
            }
            var ascent = font.ascent, descent = font.descent, charGap = font.charGap;
            if (typeof font.lineHeight !== 'number') {
                font.lineHeight = ascent + descent + 1;
            }
            var chars = font.chars;
            var newChars = [];
            var pos = 0;
            for (var j = 0; j < chars.length; j++) {
                if (typeof chars[j] === 'number') {
                    pos = chars[j];
                }
                else {
                    var ch = chars[j];
                    newChars[pos++] = ch;
                    if (typeof ch.a !== 'number') {
                        ch.a = ch.w + charGap;
                    }
                    if (typeof ch.xo !== 'number') {
                        ch.xo = 0;
                    }
                    ch.yo = ascent - ch.h - (typeof ch.up === 'number' ? ch.up : 0);
                }
            }
            font.chars = newChars;
        }
        
        // dialogs
        var dialogs = new Array(gameDef.dialogs.length);
        this.dialog = dialogs;
        for (var i = 0; i < gameDef.dialogs.length; i++) {
            var dialogDef = gameDef.dialogs[i];
            var dialog = new ags.Dialog(engine, dialogDef, i);
            dialogs[i] = dialog;
            this[dialogDef.scriptName] = dialog;
        }
        
        // guis
        var guis = new Array(gameDef.guis.length);
        this.gui = guis;
        for (var i = 0; i < gameDef.guis.length; i++) {
            var guiDef = gameDef.guis[i];
            var gui = new ags.GUI(engine, guiDef, i);
            guis[i] = gui;
            this[guiDef.scriptName] = gui;
            for (var j = 0; j < guiDef.controls.length; j++) {
                var controlDef = guiDef.controls[j];
                var control;
                switch(controlDef.type) {
                    case "label":
                        control = new ags.GUI.Label(engine, controlDef, gui, j);
                        break;
                }
                gui.controls[j] = control;
                this[controlDef.scriptName] = control;
            }
        }
        
        // characters
        var characters = new Array(gameDef.characters.length);
        this.character = characters;
        for (var i = 0; i < gameDef.characters.length; i++) {
            var characterDef = gameDef.characters[i];
            var newCharacter = new ags.Character(engine, characterDef, i);
            characters[i] = newCharacter;
            this[characterDef.scriptName] = newCharacter;
        }
        this.player = characters[gameDef.playerCharacter];
        
        // audio
        var audioDef = gameDef.audio;
        for (var i = 0; i < audioDef.clips.length; i++) {
            var clipDef = audioDef.clips[i];
            var newClip = new ags.AudioClip(this, clipDef);
            this[clipDef.scriptName] = newClip;
        }
        
        // mouse cursors
        var mouseCursors = new Array(gameDef.mouseCursors.length);
        this.mouseCursors = mouseCursors;
        for (var i = 0; i < gameDef.mouseCursors.length; i++) {
            mouseCursors[i] = new ags.MouseCursor(this, gameDef.mouseCursors[i], i);
        }
        
        // script objects
        this.mouse = engine.mouse;
        this.game = {
            score: 0,
            used_mode: 0,
            disabled_user_interface: 0,
            gscript_timer: 0,
            debug_mode: 0,
            globalvars: ags.util.fillArray(50, 0),
            messagetime: 0,
            usedinv: 0,
            top_inv_item: 0,
            num_inv_displayed: 0,
            num_inv_items: 0,
            items_per_line: 0,
            text_speed: 15,
            sierra_inv_color: 0,
            talkanim_speed: 0,
            inv_item_wid: 0,
            inv_item_hit: 0,
            text_shadow_color: 0,
            swap_portrait: 0,
            speech_text_gui: 0,
            following_room_timer: 0,
            total_score: 0,
            skip_display: 0,
            no_multiloop_repeat: 0,
            roomscript_finished: 0,
            inv_activated: 0,
            no_textbg_when_voice: 0,
            max_dialogoption_width: 0,
            no_hicolor_fadein: 0,
            bgspeech_game_speed: 0,
            bgspeech_stay_on_display: 0,
            unfactor_speech_from_textlength: 0,
            mp3_loop_before_end: 0,
            speech_music_drop: 0,
            in_cutscene: 0,
            skipping_cutscene: 0,
            room_width: 0,
            room_height: 0,
            game_speed_modifier: 0,
            score_sound: 0,
            previous_game_data: 0,
            replay_hotkey: 0,
            dialog_options_x: 0,
            dialog_options_y: 0,
            narrator_speech: 0,
            ambient_sounds_persist: 0,
            lipsync_speed: 0,
            close_mouse_end_speech_time: 0,
            disabling_antialiasing: 0,
            text_speed_modifier: 0,
            text_align: 0,
            speech_bubble_width: 0,
            min_dialogoption_width: 0,
            disable_dialog_parser: 0,
            anim_background_speed: 0,
            top_bar_backcolor: 0,
            top_bar_textcolor: 0,
            top_bar_bordercolor: 0,
            top_bar_borderwidth: 0,
            top_bar_ypos: 0,
            screenshot_width: 0,
            screenshot_height: 0,
            top_bar_font: 0,
            speech_text_align: 0,
            auto_use_walkto_points: 0,
            inventory_greys_out: 0,
            skip_speech_specific_key: 0,
            abort_key: 0,
            fade_color_red: 0,
            fade_color_green: 0,
            fade_color_blue: 0,
            show_single_dialog_option: 0,
            keep_screen_during_instant_transition: 0,
            read_dialog_option_color: 0,
            stop_dialog_at_end: 0
        };
        this.system = {
            screen_width: 0,
            screen_height: 0,
            color_depth: 0,
            os: 0,
            windowed: 0,
            vsync: 0,
            viewport_width: 0,
            viewport_height: 0,
            version: ags.util.fillArray(10, 0)
        };
        
    };
    ags.Game.prototype = {
				baseURL: "",
        getResourceLocation: function(resourceName, params) {
            return this.baseURL + this.def.resourceLocations[resourceName].replace(/\{\{(.*?)\}\}/g, function(_,param) { return params[param]; });
        },
        save: function() {
            var data = {};
            return data;
        },
        init: function(startList) {
            
            var game = this;
            var gameDef = this.def;
            var engine = this.engine;
            
            // fonts
            var fonts = new Array(gameDef.fonts.length);
            this.fonts = fonts;
            for (var i = 0; i < gameDef.fonts.length; i++) {
                if (gameDef.fonts[i]) {
                    (function(num){
                    
                        var fontDef = gameDef.fonts[i];
                        startList.add("font");
                        engine.renderer.loadImage(game.getResourceLocation("font", {fontNumber:num}),
                            function(img) {
                                fonts[num] = engine.renderer.createFont(fontDef, img, num);
                                startList.checkOff("font");
                            }, fontDef.imageWidth, fontDef.imageHeight);
                        
                    })(i);
                }
            }
            
        }
    };

    ags.Character = function(engine, charDef) {
        this.engine = engine;
        this.def = charDef;
        this.x = charDef.x;
        this.y = charDef.y;
        this.clickable = charDef.clickable;
        this.room = charDef.room;
        this.view = charDef.normalView;
        this.normalView = engine.game.views[charDef.normalView];
        this.animationDelay = charDef.animationDelay;
        this.movementSpeed = charDef.movementSpeed;
    };
    ags.Character.prototype = {
        engine: null,
        transparency: 0,
        sprite: null,
        loop: 0,
        frame: 0,
        baseline: 0,
        normalView: null,
        animationDelay: 0,
        addToChecklist: function(checklist) {
            if (this.normalView) {
                this.normalView.addToChecklist(checklist);
            }
        },
        deliverSprite: function() {
            if (this.sprite) return;
            var engine = this.engine;
            var sprite = engine.renderer.createSprite();
            sprite.represents = this;
            sprite.setParams({
                x:this.x,
                y:this.y,
                clickable: this.clickable,
                baseline: this.baseline,
                multX: -0.5,
                multY: -1,
                image: engine.game.def.views[this.view].loops[this.loop].frames[this.frame].image,
                alpha:engine.transparencyToAlpha(this.transparency)
            });
            engine.renderer.addSpriteToLayer(sprite, 'scene');
            this.sprite = sprite;
        },
        withdrawSprite: function() {
            if (!this.sprite) return;
            var engine = this.engine;
            engine.renderer.removeSpriteFromLayer(this.sprite, 'scene');
            this.sprite = null;
        }
    };

    ags.InventoryItem = function(gameState, itemDef) {
    };
    ags.InventoryItem.prototype = {
    };
    
    ags.MouseCursor = function(game, cursorDef, number) {
        this.number = number;
        this.def = cursorDef;
    };
    ags.MouseCursor.prototype = {
    };
    
    ags.AudioClip = function(game, clipDef) {
        this.def = clipDef;
    };
    
    ags.AnimationView = function(engine, viewDef, number) {
        this.number = number;
        
        var loops = [];
        this.loops = loops;
        for (var i = 0; i < viewDef.loops.length; i++) {
            loops[i] = new ags.AnimationLoop(engine, viewDef.loops[i], this, i);
        }
    };
    ags.AnimationView.prototype = {
        loops: null,
        addToChecklist: function(checklist) {
            for (var i = 0; i < this.loops.length; i++) {
                this.loops[i].addToChecklist(checklist);
            }
        }
    };
    
    ags.AnimationLoop = function(engine, loopDef, view, number) {
        this.engine = engine;
        this.view = view;
        this.number = number;
        
        var frames = [];
        this.frames = frames;
        for (var i = 0; i < loopDef.frames.length; i++) {
            frames[i] = new ags.AnimationFrame(engine, loopDef.frames[i], this, i);
        }
    };
    ags.AnimationLoop.prototype = {
        frames: null,
        addToChecklist: function(checklist) {
            for (var i = 0; i < this.frames.length; i++) {
                this.frames[i].addToChecklist(checklist);
            }
        }
    };
    
    ags.AnimationFrame = function(engine, frameDef, loop, number) {
        this.engine = engine;
        this.loop = loop;
        this.number = number;
        this.image = frameDef.image;
        this.flipped = frameDef.flipped;
        this.delay = frameDef.delay;
    };
    ags.AnimationFrame.prototype = {
        engine: null,
        image: -1,
        addToChecklist: function(checklist) {
            this.engine.renderer.addNumberedImageToChecklist(checklist, this.image);
        }
    };
    
    ags.ScriptContext = function(engine, func, params) {
        this.engine = engine;
        this.func = func;
        this.reset(params);
    };
    ags.ScriptContext.prototype = {
        engine: null,
        func: null,
        vars: null,
        stack: null,
        queue: null,
        blocking: false,
        blockedOn: null,
        entryPoint: 0,
        reset: function(params) {
            var vars = {};
            this.vars = vars;
            var func = this.func;
            if (params) {
                var paramNames = func.paramNames;
                for (var i = 0; i < paramNames.length; i++) {
                    vars[paramNames[i]] = params[i];
                }
            }
            this.stack = [];
            this.queue = [];
            this.entryPoint = 0;
        },
        returnValue: null,
        finish: function(returnValue) {
            this.returnValue = returnValue;
            this.entryPoint = -1;
            return -1;
        },
        nextEntryPoint: function(entryPoint) {
            this.entryPoint = entryPoint;
            return entryPoint;
        },
        isBlocked: function() {
            return !!(this.blockedOn || this.blocking);
        },
        beginCall: function(func, params) {
            this.queue.push([func, params, true]);
        },
        queueCall: function(func, params) {
            this.queue.push([func, params, false]);
        },
        awaitingValue: false,
        run: function() {
            var vars = this.vars;
            var stack = this.stack;
            var queue = this.queue;
            var func = this.func;
            var engine = this.engine;
            while (1) {
                if (this.blockedOn) {
                    if (this.blockedOn.run()) {
                        if (this.awaitingValue) {
                            stack.push(this.blockedOn.returnValue);
                            this.awaitingValue = false;
                        }
                        this.blockedOn = null;
                    }
                    else {
                        return false;
                    }
                }
                else if (this.blocking) {
                    return false;
                }
                else if (queue.length > 0) {
                    var dequeued = queue.shift();
                    var bfunc = dequeued[0];
                    var params = dequeued[1];
                    if (typeof bfunc.blocking !== 'undefined' && bfunc.blocking) {
                        this.awaitingValue = dequeued[2];
                        this.blockedOn = new ags.ScriptContext(this.engine, bfunc, params);
                    }
                    else {
                        if (dequeued[2]) {
                            var retValue = bfunc.apply(engine, params || null);
                            if (typeof retValue === 'undefined') {
                                retValue = null;
                            }
                            stack.push(retValue);
                        }
                        else {
                            bfunc.apply(engine, params || []);
                        }
                    }
                }
                else if (this.entryPoint === -1 || !func) {
                    return true;
                }
                else {
                    func(this, stack, vars);
                }
            }
        }
        
    };
    
    ags.Dialog = function(engine, dialogDef, number) {
        this.engine = engine;
        this.number = number;
        var options = new Array(dialogDef.options.length);
        this.options = options;
        options[0] = null;
        for (var i = 1; i < dialogDef.options.length; i++) {
            var optionDef = dialogDef.options[i];
            options[i] = {
                number: i,
                dialog: this,
                text:optionDef.text,
                state:optionDef.show ? 1 /* eOptionOn */ : 0 /* eOptionOff */,
                say:optionDef.say
            };
        }
    };
    ags.Dialog.prototype = {
        number: 0,
        options: null
    };
    
    ags.GUI = function(engine, guiDef, number) {
        this.def = guiDef;
        switch(guiDef.visibility) {
            case "normalInitiallyOn":
            case "alwaysShown":
                this.visible = true;
                break;
            default:
                this.visible = false;
                break;
        }
        this.engine = engine;
        this.number = number;
        this.controls = [];
    };
    ags.GUI.prototype = {
        def: null,
        engine: null,
        number: -1,
        controls: null,
        visible: false,
        setParam: function(paramName, paramValue) {
            this[paramName] = paramValue;
            $(this).trigger('changedParam', [paramName, paramValue]);
        }
    };
    
    ags.GUI.Label = function(engine, controlDef, gui, number) {
        this.engine = engine;
        this.def = controlDef;
        this.number = number;
        this.text = '';
        this.gui = gui;
        var label = this;
        $(engine).bind("mouseMovedOver", function(e, newThing) {
            var name = newThing ? newThing.def.name || '' : '';
            label.setParam('text', name);
        });
    };
    ags.GUI.Label.prototype = {
        engine: null,
        def: null,
        number: -1,
        gui: null,
        setParam: function(paramName, paramValue) {
            this[paramName] = paramValue;
            $(this).trigger('changedParam', [paramName, paramValue]);
        }
    };
    
    ags.Font = function(engine, fontDef, number) {
        this.engine = engine;
        this.def = fontDef;
        this.number = number;
    };
    ags.Font.prototype = {
        // splits on [
        // changes \[ to [
        // wraps on pixel widths
        splitLines: function(message, firstLineWidth, afterFirstLineWidth) {
            var bracketRex = /\\?\[/g;
            bracketRex.lastIndex = 0;
            var startIndex = 0;
            var lines = [];
            for (var bracketMatch = bracketRex.exec(message); bracketMatch; bracketMatch = bracketRex.exec(message)) {
                if (bracketMatch[0].length === 1) {
                    lines.push(message.slice(startIndex, bracketMatch.index).replace(/\\\[/g, '['));
                    startIndex = bracketMatch.index + 1;
                }
            }
            lines.push(message.slice(startIndex).replace(/\\\[/g, '['));
            
            if (typeof firstLineWidth !== 'number') {
                return lines;
            }
            if (typeof afterFirstLineWidth !== 'number') {
                afterFirstLineWidth = firstLineWidth;
            }
            
            var spaceRex = /(\s+)\S+/g;
            spaceRex.lastIndex = 0;
            startIndex = 0;
            
            var width = firstLineWidth;
            for (var i = 0; i < lines.length; i++) {
                var line = lines.splice(i--, 1)[0];
                for (var spaceMatch = spaceRex.exec(line); spaceMatch; spaceMatch = spaceRex.exec(line)) {
                    if (this.stringWidth(line.slice(startIndex, spaceMatch.index + spaceMatch[0].length)) > width) {
                        width = afterFirstLineWidth;
                        lines.splice(++i, 0, line.slice(startIndex, spaceMatch.index));
                        startIndex = spaceMatch.index + spaceMatch[1].length;
                    }
                }
                lines.splice(++i, 0, line.slice(startIndex));
            }
            return lines;
        },
        stringWidth: function(str) {
            var width = 0;
            var def = this.def;
            var chars = def.chars;
            for (var i = 0; i < str.length; i++) {
                var c = str.charCodeAt(i);
                if (c < chars.length && chars[c]) {
                    c = chars[c];
                    width += c.a;
                }
            }
            return width;
        }
    };
    
    ags.NullFileSystem = function() {
    };
    ags.NullFileSystem.prototype = {
        deleteFile: function(filename) {
            return false;
        },
        fileExists: function(filename) {
            return false;
        },
        openFile: function(filename, mode) {
            /*
                If this did return a file object, it must implement these methods:
                file.close();
                file.isEof();
                file.isError();
                [readable file only:]
                file.readInt();
                file.readRawChar();
                file.readString();
                file.readRawInt();
                file.readRawLine();
                [writable file only:]
                file.writeInt();
                file.writeString();
                file.writeRawLine();
                file.writeRawChar();
            */
            switch(mode) {
                case "read":
                    return null;
                case "write":
                    return null;
                case "append":
                    return null;
            }
        }
    };
    
    ags.File = function() {
    };
    ags.File.prototype = {
        error: null,
        close: function() {
            $(this).trigger('closing');
        },
        readInt: function() {
            this.error = "cannot read int";
            return 0;
        },
        readRawChar: function() {
            this.error = "cannot read char";
            return 0;
        },
        readRawInt: function() {
            this.error = "cannot read raw int";
            return 0;
        },
        readRawLine: function() {
            this.error = "cannot read raw line";
            return "";
        },
        readString: function() {
            this.error = "cannot read string";
            return "";
        },
        readAllText: function() {
            this.error = "cannot read all text";
            return "";
        },
        writeInt: function(v) {
            this.error = "cannot write int";
        },
        writeRawChar: function(v) {
            this.error = "cannot write raw char";
        },
        writeRawLine: function(v) {
            this.error = "cannot write raw line";
        },
        writeRawString: function(v) {
            this.error = "cannot write raw string";
        },
        writeString: function(v) {
            this.error = "cannot write string";
        },
        isEof: function() {
            return true;
        },
        isError: function() {
            return !!this.error;
        }
    };
    
    ags.ReadStringFile = function(str) {
        ags.File.call(this);
        this.str = str;
        this.pos = 0;
    };
    ags.ReadStringFile.prototype = $.extend({}, ags.File.prototype, {
        readInt: function() {
            var str = this.str, pos = this.pos;
            if (pos+5 >= str.length || str.charCodeAt(pos) !== 49) {
                this.error = 'unable to read int';
                return 0;
            }
            this.pos = pos + 5;
            return str.charCodeAt(pos+1) | (str.charCodeAt(pos+2) << 8) | (str.charCodeAt(pos+3) << 16) | (str.charCodeAt(pos+4) << 24);
        },
        readRawChar: function() {
            var str = this.str, pos = this.pos;
            if (pos >= str.length) {
                this.error = 'unable to read char';
                return 0;
            }
            return str.charCodeAt(this.pos++);
        },
        readRawInt: function() {
            var str = this.str, pos = this.pos;
            if (pos+4 >= str.length) {
                this.error = 'unable to read raw int';
                return 0;
            }
            this.pos = pos + 4;
            return str.charCodeAt(pos) | (str.charCodeAt(pos+1) << 8) | (str.charCodeAt(pos+2) << 16) | (str.charCodeAt(pos+3) << 24);
        },
        readAllText: function() {
            var text = this.str.slice(this.pos);
            this.pos = this.str.length;
            return text;
        },
        readRawLineBack: function() {
            var lineRex = /([^\r\n]*)(\r\n|\n|\r)?/g;
            lineRex.lastIndex = this.pos;
            var match = lineRex.exec(this.str);
            this.pos += match[0].length;
            return match[1];
        },
        readStringBack: function() {
            var str = this.str, pos = this.pos;
            if (pos+4 >= str.length) {
                this.error = 'unable to read string';
                return 0;
            }
            var len = str.charCodeAt(pos) | (str.charCodeAt(pos+1) << 8) | (str.charCodeAt(pos+2) << 16) | (str.charCodeAt(pos+3) << 24);
            pos += 4;
            if (pos+len >= str.length) {
                this.error = 'unable to read string';
                return 0;
            }
            this.pos = pos + len;
            return str.slice(pos, pos + len - 1);
        },
        isEof: function() {
            return this.pos >= this.str.length;
        }
    });
    
    ags.WriteStringFile = function(appendTo) {
        ags.File.call(this);
        this.data = [];
        if (typeof appendTo === 'string') {
            this.data.push(appendTo);
        }
    };
    ags.WriteStringFile.prototype = $.extend({}, ags.File.prototype, {
        writeInt: function(v) {
            this.data.push(String.fromCharCode(49, v & 0xff, (v >> 8) & 0xff, (v >> 16) & 0xff, (v >> 24) & 0xff));
        },
        writeRawChar: function(v) {
            this.data.push(String.fromCharCode(v));
        },
        writeRawLine: function(line) {
            this.data.push(line + "\r\n");
        },
        writeRawString: function(str) {
            this.data.push(str);
        },
        writeString: function(str) {
            var v = str.length + 1;
            this.data.push(String.fromCharCode(v & 0xff, (v >> 8) & 0xff, (v >> 16) & 0xff, (v >> 24) & 0xff) + str + '\x00');
        },
        close: function() {
            $(this).trigger('closing', [this.data.join('')]);
        }
    });

});
