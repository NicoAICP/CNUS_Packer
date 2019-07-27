using System.Collections.Generic;

namespace CNUS_packer.packaging
{
    public class ContentRule
    {
        public string pattern { get; }
        public ContentDetails details { get; }
        public bool contentPerMatch { get; }

        private ContentRule(string pattern, ContentDetails details, bool contentPerMatch = false)
        {
            this.pattern = pattern;
            this.details = details;
            this.contentPerMatch = contentPerMatch;
        }

        public static List<ContentRule> GetCommonRules(short contentGroup, long titleID)
        {
            List<ContentRule> commonRules = new List<ContentRule>();
            ContentDetails common_details_code = new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code);
            ContentDetails common_details_meta = new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta);
            ContentDetails common_details_preload = new ContentDetails(true, Settings.groupid_code, 0x0L, Settings.fstflags_code);
            ContentDetails common_details_content = new ContentDetails(true, contentGroup, titleID, Settings.fstflags_content);

            commonRules.Add(new ContentRule(@"^/code/app\.xml$", common_details_code));
            commonRules.Add(new ContentRule(@"^/code/cos\.xml$", common_details_code));

            commonRules.Add(new ContentRule(@"^/meta/meta\.xml$", common_details_meta));
            commonRules.Add(new ContentRule(@"^/meta/((?!\.xml).)*$", common_details_meta));
            commonRules.Add(new ContentRule(@"^/meta/bootMovie\.h264$", common_details_meta));
            commonRules.Add(new ContentRule(@"^/meta/bootLogoTex\.tga$", common_details_meta));
            commonRules.Add(new ContentRule(@"^/meta/Manual\.bfma$", common_details_meta));
            commonRules.Add(new ContentRule(@"^/meta/.*\.jpg$", common_details_meta));

            commonRules.Add(new ContentRule(@"/code/.*(\.rpx|\.rpl)$", common_details_code, true));

            commonRules.Add(new ContentRule(@"^/code/preload\.txt$", common_details_preload));

            commonRules.Add(new ContentRule(@"^/code/fw\.img$", common_details_code));
            commonRules.Add(new ContentRule(@"^/code/fw\.tmd$", common_details_code));
            commonRules.Add(new ContentRule(@"^/code/htk\.bin$", common_details_code));
            commonRules.Add(new ContentRule(@"^/code/rvlt\.tik$", common_details_code));
            commonRules.Add(new ContentRule(@"^/code/rvlt\.tmd$", common_details_code));

            commonRules.Add(new ContentRule(@"^/content/.*$", common_details_content));

            return commonRules;
        }
    }
}
