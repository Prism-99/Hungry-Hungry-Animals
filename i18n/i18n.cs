using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealFarmAnimals.I18n
{
    internal class i18n
    {
        private static ITranslationHelper Translations;
        public static void Init(ITranslationHelper translations)
        {
            Translations = translations;
        }
        public static Translation GetByKey(string key, object tokens = null)
        {
            if (Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(Init)} from the mod's entry method before reading translations.");
            return Translations.Get(key, tokens);
        }

        public static string Title()
        {
            return GetByKey("gmcm.title");
        }
        public static string Enabled()
        {
            return GetByKey("gmcm.enabled");
        }
        public static string Chance()
        {
            return GetByKey("gmcm.chance");
        }
        public static string Chance_TT()
        {
            return GetByKey("gmcm.chance.tt");
        }
        public static string Eating()
        {
            return GetByKey("gmcm.eating");
        }
        public static string Eating_TT()
        {
            return GetByKey("gmcm.eating.tt");
        }
        public static string Message()
        {
            return GetByKey("gmcm.message");
        }
        public static string Message_TT()
        {
            return GetByKey("gmcm.message.tt");
        }
        public static string CropsEaten(int eaten)
        {
            return GetByKey("livestock.ate", new {cropseaten=eaten});
        }
    }
}
