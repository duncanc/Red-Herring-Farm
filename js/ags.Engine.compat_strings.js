
(function(){

    var engine = ags.Engine.prototype;

    // String Buffer Utilities
    engine.StrCopy = function(buffer, newValue)        { buffer.value = newValue;                                        };
    engine.StrCat = function(buffer, add)              { buffer.value += add;                                            };
    engine.StrFormat = function(buffer, text, fmtArgs) { buffer.value = this.String$$Format(text, fmtArgs);              };
    engine.StrSetCharAt = function(buffer, i, ch)      { buffer.value = this.String$$ReplaceCharAt(buffer.value, i, ch); };
    engine.StrToLowerCase = function(buffer)           { buffer.value = buffer.value.toLowerCase();                      };
    engine.StrToUpperCase = function(buffer)           { buffer.value = buffer.value.toUpperCase();                      };
    engine.StrComp = function(str, other)              { return this.String$$CompareTo(str, other, true);                };
    engine.StrCaseComp = function(str, other)          { return this.String$$CompareTo(str, other, false);               };
    engine.StrLen = engine.String$$get_Length;
    engine.StrContains = engine.String$$Contains;
    
    engine.StringToInt = engine.String$$AsInt;

    engine.SetGlobalString = engine.Game$$seti_GlobalStrings;
    engine.GetGlobalString = function(stringID, buffer) { buffer.value = this.Game$$geti_GlobalStrings(stringID); };
    engine.InputBox = function(prompt, buffer) { buffer.value = this.Game$$InputBox(prompt); };
    engine.GetTranslationName = function(buffer) { buffer.value = this.Game$$get_TranslationFilename; return 1; },
    GetSaveSlotDescription: function(saveSlot, buffer) { buffer.value = this.Game$$GetSaveSlotDescription(saveSlot); },
    GetLocationName: function(x, y, buffer) {
        buffer.value = this.Game$$GetLocationName(x,y);
    },
    GetRoomPropertyText: function(property, buffer) {
        buffer.value = this.Room$$GetTextProperty(property);
    },
    
    // Parser
    ParseText:               function(text)       { this.Parser$$ParseText(text); },
    SaidUnknownWord: function(buffer) {
        var word = this.Parser$$SaidUnknownWord();
        if (word) {
            buffer.value = word;
            return 1;
        }
        return 0;
    },
    
    File$$ReadRawLine: function(file, buffer) { buffer.value = this.File$$ReadRawLineBack(file); },
    File$$ReadString: function(file, buffer) { buffer.value = this.File$$ReadStringBack(file); },
    
  void InventoryItem$$GetName(string buffer);   
  void InventoryItem$$SetName(const string newName);
    void InventoryItem$$GetPropertyText(const string property, string buffer);

  void TextBox$$SetText(tb, const string text);
  void TextBox$$GetText(tb, string buffer);
  
  void Label$$GetText(l, string buffer);
  void Label$$SetText(l, const string text);
  
  void Button$$GetText(string buffer);
  void Button$$SetText(const string text);
	void ListBox$$GetItemText(int listIndex, string buffer);
	void ListBox$$SetItemText(int listIndex, const string newText);
    
  void Hotspot$$GetName(string buffer);
    void Hotspot$$GetPropertyText(const string property, string buffer);
    
	void     Object$$GetName(string buffer);
    function Object$$GetPropertyText(const string property, string buffer);
    
    Character$$GetPropertyText: function(chr, property, buffer) {
        buffer.value = this.Character$$GetTextProperty(chr, property);
    },
    
    void GetMessageText (int messageNumber, string buffer);
    
})();
