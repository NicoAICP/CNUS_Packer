using System.Collections.Generic;

namespace CNUSPACKER.packaging
{
    public class ContentRule
    {
        public readonly string pattern;
        public readonly ContentDetails details;
        public readonly bool contentPerMatch;

        private ContentRule(string pattern, ContentDetails details, bool contentPerMatch = false)
        {
            this.pattern = pattern;
            this.details = details;
            this.contentPerMatch = contentPerMatch;
        }

        public static List<ContentRule> GetCommonRules(short contentGroup, long titleID)
        {
            List<ContentRule> commonRules = new List<ContentRule>();
            ContentDetails commonDetailsCode = new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code);
            ContentDetails commonDetailsMeta = new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta);
            ContentDetails commonDetailsPreload = new ContentDetails(true, Settings.groupid_code, 0x0L, Settings.fstflags_code);
            ContentDetails commonDetailsContent = new ContentDetails(true, contentGroup, titleID, Settings.fstflags_content);

            commonRules.Add(new ContentRule(@"^/code/app\.xml$", commonDetailsCode));
            commonRules.Add(new ContentRule(@"^/code/cos\.xml$", commonDetailsCode));

            commonRules.Add(new ContentRule(@"^/meta/meta\.xml$", commonDetailsMeta));
            commonRules.Add(new ContentRule(@"^/meta/((?!\.xml).)*$", commonDetailsMeta));
            commonRules.Add(new ContentRule(@"^/meta/bootMovie\.h264$", commonDetailsMeta));
            commonRules.Add(new ContentRule(@"^/meta/bootLogoTex\.tga$", commonDetailsMeta));
            commonRules.Add(new ContentRule(@"^/meta/Manual\.bfma$", commonDetailsMeta));
            commonRules.Add(new ContentRule(@"^/meta/.*\.jpg$", commonDetailsMeta));

            commonRules.Add(new ContentRule(@"/code/.*(\.rpx|\.rpl)$", commonDetailsCode, true));

            commonRules.Add(new ContentRule(@"^/code/preload\.txt$", commonDetailsPreload));

            commonRules.Add(new ContentRule(@"^/code/fw\.img$", commonDetailsCode));
            commonRules.Add(new ContentRule(@"^/code/fw\.tmd$", commonDetailsCode));
            commonRules.Add(new ContentRule(@"^/code/htk\.bin$", commonDetailsCode));
            commonRules.Add(new ContentRule(@"^/code/rvlt\.tik$", commonDetailsCode));
            commonRules.Add(new ContentRule(@"^/code/rvlt\.tmd$", commonDetailsCode));

            commonRules.Add(new ContentRule(@"^/content/.*$", commonDetailsContent));

            return commonRules;
        }
    }
}
