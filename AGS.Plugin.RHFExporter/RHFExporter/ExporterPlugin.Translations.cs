using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm
{
    public partial class ExporterPlugin
    {
        public void ExportTranslation(AGS.Types.Translation translation)
        {
            using (JsonWriter output = JsonWriter.Create(InExportFolder(TRANSLATION_FILENAME, translation.Name)))
            {
                //output.ObfuscateKeys = true;
                //output.ObfuscateValues = true;
                WriteTranslationJson(output, translation);
            }
        }
        public void WriteTranslationJson(JsonWriter output, AGS.Types.Translation translation)
        {
            WriteTranslationJson(output, null, translation);
        }
        public void WriteTranslationJson(JsonWriter output, string key, AGS.Types.Translation translation)
        {
            using (output.BeginObject(key))
            {
                output.WriteValue("game", GetCurrentGameGuid());
                output.WriteValue("normalFont", translation.NormalFont);
                output.WriteValue("speechFont", translation.SpeechFont);
                using (output.BeginObject("translatedLines"))
                {
                    foreach (KeyValuePair<string, string> line in translation.TranslatedLines)
                    {
                        if (!String.IsNullOrEmpty(line.Value))
                        {
                            output.WriteValue(line.Key, line.Value);
                        }
                    }
                }
            }
        }
    }
}
