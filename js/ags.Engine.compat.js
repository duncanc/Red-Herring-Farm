
(function(){

    var engine = ags.Engine.prototype;
    
    // Viewport
    engine.GetCharacterAt = function(x,y) { var chr = this.Character$$GetAtScreenXY(x,y);     return chr ? chr.number : 0; };
    engine.GetHotspotAt   = function(x,y) { var hsp = this.Hotspot$$GetAtScreenXY(x,y);       return hsp ? hsp.number : 0; };
    engine.GetRegionAt    = function(x,y) { var reg = this.Region$$GetAtScreenXY(x,y);        return reg ? reg.number : 0; };
    engine.GetInvAt       = function(x,y) { var inv = this.InventoryItem$$GetAtScreenXY(x,y); return inv ? inv.number : 0; };
    
    // Overlays
    int CreateGraphicOverlay(int x, int y, int slot, bool transparent);
    int CreateTextOverlay(int x, int y, int width, FontType, int colour, const string text, ...);
    void SetTextOverlay(int overlayID, int x, int y, int width, FontType, int colour, const string text, ...);
    void RemoveOverlay(int overlayID);
    int IsOverlayValid(int overlayID);
    int MoveOverlay(int overlayID, int x, int y);
    
    // Built-in GUIs
    InventoryScreen: function() { /* TODO */ return 0; },
    void SetInvDimensions(int width, int height);
    
    // Mouse Cursors
    engine.ChangeCursorGraphic = engine.Mouse$$ChangeModeGraphic;
    engine.GetCursorMode = engine.Mouse$$get_Mode;
    engine.SetCursorMode = engine.Mouse$$set_Mode;
    engine.ChangeCursorHotspot = engine.Mouse$$ChangeModeHotspot;
    engine.EnableCursorMode = engine.Mouse$$EnableMode;
    engine.DisableCursorMode = engine.Mouse$$DisableMode;
    engine.SaveCursorForLocationChange = engine.Mouse$$SaveCursorUntilItLeaves;
    engine.SetNextCursorMode = engine.Mouse$$SelectNextMode;
    engine.SetDefaultCursor = engine.Mouse$$SetDefaultCursor;
    engine.SetMouseCursor = engine.Mouse$$UseModeGraphic;
    engine.ShowMouseCursor = function() { this.Mouse$$set_Visible(1); },
    engine.HideMouseCursor = function() { this.Mouse$$set_Visible(0); },
    
    // Mouse Buttons
    engine.IsButtonDown = engine.Mouse$$IsButtonDown;
    
    var numberedObjectFunc = function(func) {
        return function(num) {
            var args = new Array(arguments.length);
            args[0] = this.getNumberedObject(num);
            for (var i = 1; i < arguments.length; i++) {
                args[i] = arguments[i];
            }
            return func.apply(this, args);
        };
    };
    
    engine.MergeObject = numberedObjectFunc(engine.Object$$MergeIntoBackground);
    engine.SetObjectTint = numberedObjectFunc(engine.Object$$Tint);
    engine.RemoveObjectTint = numberedObjectFunc(engine.Object$$RemoveTint);
    
void StopObjectMoving(int object);
void AnimateObject(int object, int loop, int delay, int repeat);
void AnimateObjectEx(int object, int loop, int delay, int repeat, int direction, int blocking);
    engine.ObjectOn = function(objNumber) { this.Object$$set_Visible(this.getNumberedObject(objNumber), 1); };
    engine.ObjectOff = function(objNumber) { this.Object$$set_Visible(this.getNumberedObject(objNumber), 0); };
void SetObjectBaseline(int object, int baseline);
int GetObjectBaseline(int object);
void SetObjectFrame(int object, int view, int loop, int frame);
void SetObjectGraphic(int object, int spriteSlot);
void SetObjectView(int object, int view);
void SetObjectTransparency(int object, int amount);
void MoveObject(int object, int x, int y, int speed);
void MoveObjectDirect(int object, int x, int y, int speed);
void SetObjectPosition(int object, int x, int y);
int AreObjectsColliding(int object1, int object2);
void GetObjectName(int object, string buffer);
int GetObjectX(int object);
int GetObjectY(int object);
int GetObjectGraphic(int object);
int IsObjectAnimating(int object);
int IsObjectMoving(int object);
int IsObjectOn(int object);
void SetObjectClickable(int object, int clickable);
void SetObjectIgnoreWalkbehinds(int object, int ignore);

    AddInventory: function(itemNumber) {
        this.Character$$AddInventory(this.game.player, this.game.inventory[itemNumber], 31998 /* SCR_NO_VALUE */);
    },
    AddInventoryToCharacter: function(chrNumber, itemNumber) {
        this.Character$$AddInventory(this.game.characters[chrNumber], this.game.inventory[itemNumber], 31998 /* SCR_NO_VALUE */);
    },
    LoseInventory: function(itemNumber) {
        this.Character$$LoseInventory(this.game.player, this.game.inventory[itemNumber]);
    },
    LoseInventoryFromCharacter: function(chrNumber, itemNumber) {
        this.Character$$LoseInventory(this.getNumberedCharacter(chrNumber), this.game.inventory[itemNumber]);
    },
    
void NewRoom(int roomNumber);
void NewRoomEx(int roomNumber, int x, int y);
void NewRoomNPC(CHARID, int roomNumber, int x, int y);
    SetCharacterClickable: function(chrNumber, clickable) {
        this.Character$$set_Clickable(this.getNumberedCharacter(chrNumber), clickable);
    },
    SetCharacterIgnoreLight: function(chrId, ignoreLight) {
        this.Character$$set_IgnoreLight(this.getNumberedCharacter(chrNumber), ignoreLight);
    },
    SetCharacterIgnoreLight: function(chrId, ignoreWBs) {
        this.Character$$set_IgnoreWalkbehinds(this.getNumberedCharacter(chrNumber), ignoreWBs);
    },
    SetCharacterProperty: function(chrNumber, int property, int newValue) {
        var chr = this.getNumberedCharacter(chrNumber);
        switch(property) {
            case 1 /* CHAR_IGNORESCALING */: this.Character$$set_IgnoreScaling(chr, newValue); break;
            case 4 /* CHAR_NOINTERACTION */: this.Character$$set_Clickable(chr, !newValue); break;
            case 8 /* CHAR_NODIAGONAL */: this.Character$$set_DiagonalLoops(chr, !newValue); break;
            case 32 /* CHAR_IGNORELIGHT */: this.Character$$set_IgnoreLighting(chr, newValue); break;
            case 64 /* CHAR_NOTURNING */: this.Character$$set_TurnBeforeWalking(chr, !newValue); break;
            case 128 /* CHAR_IGNOREWALKBEHINDS */: this.Character$$set_IgnoreWalkbehinds(chr, newValue); break;
            case 512 /* CHAR_WALKTHROUGH */: this.Character$$set_Solid(chr, !newValue); break;
            case 1024: /* CHAR_SCALEMOVESPEED */: this.Character$$set_ScaleMoveSpeed(chr, newValue); break;
        }
    }
    
void SetActiveInventory(int item);
void MoveCharacterPath(CHARID, int x, int y);
void AnimateCharacter (CHARID, int loop, int delay, int repeat);
void AnimateCharacterEx (CHARID, int loop, int delay, int repeat, int direction, int blocking);
void FollowCharacter(CHARID sheep, CHARID shepherd);
void FollowCharacterEx(CHARID sheep, CHARID shepherd, int dist, int eagerness);
void MoveCharacterToHotspot(CHARID, int hotspot);
void MoveCharacterToObject(CHARID, int object);
void MoveToWalkableArea(CHARID);
void FaceCharacter(CHARID, CHARID toFace);
void FaceLocation(CHARID, int x, int y);
void StopMoving(CHARID);
void SetCharacterSpeed(CHARID, int speed);
void SetCharacterSpeedEx(CHARID, int x_speed, int y_speed);
void SetCharacterViewEx(CHARID, int view, int loop, int align);
void SetCharacterViewOffset(CHARID, int view, int x_offset, int y_offset);
void SetCharacterFrame(CHARID, int view, int loop, int frame);
void ReleaseCharacterView(CHARID);
void ChangeCharacterView(CHARID, int view);
void SetCharacterSpeechView(CHARID, int view);
void SetCharacterBlinkView(CHARID, int view, int interval);
void SetCharacterIdle(CHARID, int idleView, int delay);
void SetCharacterTransparency(CHARID, int transparency);
int AreCharObjColliding(CHARID, int object);
int AreCharactersColliding(CHARID, CHARID);
    void SetCharacterBaseline(CHARID, int baseline);
void MoveCharacter(CHARID, int x, int y);
void MoveCharacterDirect(CHARID, int x, int y);
void MoveCharacterBlocking(CHARID, int x, int y, int direct);
void MoveCharacterStraight(CHARID, int x, int y);
    SetCharacterView: function(chrNumber, view) {
        this.Character$$LockView(this.getNumberedCharacter(chrNumber), view);
    },
void DisplaySpeech (CHARID, const string message, ...);
void DisplaySpeechAt (int x, int y, int width, CHARID, const string message);
int DisplaySpeechBackground(CHARID, const string message);  
void DisplayThought (CHARID, const string message, ...);
    SetTalkingColor: function(chrNumber, v) {
        this.Character$$set_SpeechColor(this.getNumberedCharacter(chrNumber), v);
    },
    
    FileOpen: function(filename, modeString) {
        var f;
        switch(modeString) {
            case 'rb': f = this.File$$Open(filename, 1); break;
            case 'wb': f = this.File$$Open(filename, 2); break;
            case 'ab': f = this.File$$Open(filename, 3); break;
        }
        if (f) {
            return this.addNumberedFile(f);
        }
        return 0;
    },
    FileClose: function(fileNumber) {
        this.File$$Close(this.getNumberedFile(fileNumber));
        this.removeNumberedFile(fileNumber);
    },
    FileReadInt: function(fileNumber) { return this.File$$ReadInt(this.getNumberedFile(fileNumber)); },
    FileReadRawChar: function(fileNumber) { return this.File$$ReadRawChar(this.getNumberedFile(fileNumber)); },
    FileReadRawInt: function(fileNumber) { return this.File$$ReadInt(this.getNumberedFile(fileNumber)); },
    FileRead: function(fileNumber, buffer) { buffer.value = this.File$$ReadStringBack(this.getNumberedFile(fileNumber)); },
    FileWriteInt: function(fileNumber, value) { this.File$$WriteInt(this.getNumberedFile(fileNumber), value); },
    FileWrite(fileNumber, text) { this.File$$WriteString(this.getNumberedFile(fileNumber), text); },
    FileWriteRawLine: function(fileNumber, text) { this.File$$WriteRawChar(this.getNumberedFile(fileNumber), text); },
    FileWriteRawChar: function(fileNumber, value) { this.File$$WriteRawChar(this.getNumberedFile(fileNumber), value); },
    FileIsEOF: function(fileNumber) { return this.File$$get_EOF(this.getNumberedFile(fileNumber)); },
    FileIsError: function(fileNumber) { return this.File$$get_Error(this.getNumberedFile(fileNumber)); },
    
void GetHotspotName(int hotspot, string buffer);
void EnableHotspot(int hotspot);
void DisableHotspot(int hotspot);
int GetHotspotPointX(int hotspot);
int GetHotspotPointY(int hotspot);

void DisableRegion(int region);
void EnableRegion(int region);
void SetAreaLightLevel(int area, int lightLevel);
void SetRegionTint(int area, int red, int green, int blue, int amount);

    int GetInvProperty(int invItem, const string property);
    void GetInvPropertyText(int invItem, const string property, string buffer);
void GetInvName(int item, string buffer);
void SetInvItemName(int item, const string name);
int GetInvGraphic(int item);
void SetInvItemPic(int item, int spriteSlot);

    IsInventoryInteractionAvailable: function(itemNumber, cursorMode) { },
void RunInventoryInteraction(int item, CursorMode);

    engine.GetRawTime = function() { return (new Date().valueOf() / 1000) | 0; };
    engine.GetTime = function(whichValue) {
        switch(value) {
            case 1: return new Date().getHours();
            case 2: return new Date().getMinutes();
            case 3: return new Date().getSeconds();
            case 4: return new Date().getDate();
            case 5: return new Date().getMonth() + 1;
            case 6: return new Date().getYear();
        }
        return 0;
    };
    
    int LoadSaveSlotScreenshot(int saveSlot, int width, int height);
int LoadImageFile(const string filename);
    void DeleteSprite(int spriteSlot);
    SetSpeechFont: function(fontType) { this.Game$$set_SpeechFont(fontType); },
    SetNormalFont: function(fontType) { this.Game$$set_NormalFont(fontType); },
    GetGameParameter: function(parameter, data1, data2, data3) {
        /* TODO */
    },
void SetDialogOption(int topic, int option, DialogOptionState);
DialogOptionState GetDialogOption(int topic, int option);
void RunDialog(int topic);

    // Raw Drawing
void RawClearScreen (int colour);
void RawDrawCircle (int x, int y, int radius);
void RawDrawImage (int x, int y, int spriteSlot);
void RawDrawImageOffset(int x, int y, int spriteSlot);
void RawDrawImageResized(int x, int y, int spriteSlot, int width, int height);
void RawDrawImageTransparent(int x, int y, int spriteSlot, int transparency);
void RawDrawLine (int x1, int y1, int x2, int y2);
void RawDrawRectangle (int x1, int y1, int x2, int y2);
void RawDrawTriangle (int x1, int y1, int x2, int y2, int x3, int y3);
void RawPrint (int x, int y, const string message, ...);
void RawPrintMessageWrapped (int x, int y, int width, FontType, int messageNumber);
void RawSetColor(int colour);
void RawSetColorRGB(int red, int green, int blue);
void RawDrawFrameTransparent (int frame, int transparency);
void RawSaveScreen ();
void RawRestoreScreen ();

    GetGUIAt: function(x, y) {
        var gui = this.GUI$$GetAtScreenXY(x, y);
        return gui ? gui.number : -1;
    },
int GetGUIObjectAt(int x, int y);

void CentreGUI(int gui);
void SetGUIBackgroundPic (int gui, int spriteSlot);
void SetGUIClickable(int gui, int clickable);
void SetGUITransparency(int gui, int amount);
int IsGUIOn (int gui);
void InterfaceOn(int gui);   // $AUTOCOMPLETEIGNORE$
void InterfaceOff(int gui);  // $AUTOCOMPLETEIGNORE$
void SetGUIZOrder(int gui, int z);

void SetGUIObjectPosition(int gui, int object, int x, int y);
void SetGUIObjectSize(int gui, int object, int width, int height);
void SetGUIObjectEnabled(int gui, int object, int enable);
void SetLabelColor(int gui, int object, int colour);
void SetLabelText(int gui, int object, const string text);
void SetLabelFont(int gui, int object, FontType);
void AnimateButton(int gui, int object, int view, int loop, int delay, int repeat);
void SetButtonText(int gui, int object, const string text);
void SetButtonPic(int gui, int object, int which, int spriteSlot);
int GetButtonPic(int gui, int object, int which);
void SetSliderValue(int gui, int object, int value);
int GetSliderValue(int gui, int object);
void SetTextBoxFont(int gui, int object, FontType);
void GetTextBoxText(int gui, int object, string buffer);
void SetTextBoxText(int gui, int object, const string text);

    SetFrameSound: function(view, loop, frame, sound) {
        this.ViewFrame$$set_Sound(this.Game$$GetViewFrame(view, loop, frame), sound);
    },
    
void ListBoxAdd(int gui, int object, const string text);
void ListBoxClear(int gui, int object);
void ListBoxDirList (int gui, int object, const string fileMask);
int ListBoxSaveGameList (int gui, int object);
void ListBoxRemove (int gui, int object, int listIndex);
int ListBoxGetNumItems (int gui, int object);
void ListBoxGetItemText(int gui, int object, int listIndex, string buffer);
int ListBoxGetSelected(int gui, int object);
void ListBoxSetSelected(int gui, int object, int listIndex);
void ListBoxSetTopItem (int gui, int object, int listIndex);
    RefreshMouse: function() { this.Mouse$$Update(); },
    void SetMouseBounds(int left, int top, int right, int bottom);
    void SetMousePosition(int x, int y);

    GetCharacterProperty: function(chrNumber, property) {
        return this.Character$$GetProperty(this.game.characters[chrNumber], property);
    },
    GetCharacterPropertyText: function(chrNumber, property, buffer) [
        buffer.value = this.Character$$GetTextProperty(this.game.characters[chrNumber], property);
    },
    void GetHotspotPropertyText(int hotspot, const string property, string buffer);
    void GetObjectPropertyText(int object, const string property, string buffer);
    int GetHotspotProperty(int hotspot, const string property);

    GetPlayerCharacter: function() { return this.game.player.number; },
    void SetPlayerCharacter(CHARID);

    SetGUISize: function(guiNumber, width, height) {
        this.GUI$$SetSize(this.game.guis[guiNumber], width, height);
    },
    SetGUIPosition: function(guiNumber, x, y) {
        this.GUI$$SetPosition(this.game.guis[guiNumber], x, y);
    },
    
void RunCharacterInteraction (CHARID, CursorMode);
void RunHotspotInteraction(int hotspot, CursorMode);
void RunObjectInteraction (int object, CursorMode);
void RunRegionInteraction(int region, int event);
    GetObjectProperty: function(objNumber, const string property);
    
})()
