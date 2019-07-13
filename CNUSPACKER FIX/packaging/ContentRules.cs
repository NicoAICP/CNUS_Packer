using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;

namespace CNUS_packer.packaging
{

    public class ContentRules
    {
        
        public List<ContentRule> rules = new List<ContentRule>();

        public ContentRules()
        {

        }
        public List<ContentRule> GetRules()
        {
            return rules;
        }
        public ContentRule AddRule(ContentRule rule)
        {
            if (!rules.Contains(rule))
            {
                rules.Add(rule);
            }
            return rule;
        }
        public ContentRule CreateNewRule(string pattern, ContentDetails details, bool contentPerMatch)
        {
            ContentRule newRule = new ContentRule(pattern, details, contentPerMatch);
            rules.Add(newRule);
            return newRule;
        }
        public ContentRule CreateNewRule(string pattern, ContentDetails details)
        {
            return CreateNewRule(pattern, details, false);
        }
        public class ContentRule
        {
            private string pattern = "";
            private ContentDetails details = null;
            private bool contentPerMatch = false;
            public ContentRule(string pattern, ContentDetails details, bool contentPerMatch)
            {
                setPattern(pattern);
                setDetails(details);
                setContentPerMatch(contentPerMatch);
            }
            public string getPattern()
            {
                return pattern;
            }
            public void setPattern(string pattern)
            {
                this.pattern = pattern;
            }
            public ContentDetails getDetails()
            {
                return details;
            }

            public void setDetails(ContentDetails details)
            {
                this.details = details;
            }

            public bool isContentPerMatch()
            {
                return contentPerMatch;
            }

            public void setContentPerMatch(bool contentPerMatch)
            {
                this.contentPerMatch = contentPerMatch;
            }
        }
        public static ContentRules getCommonRules(short contentGroup, long titleID)
        {
            ContentRules rules = new ContentRules();
            ContentDetails common_details_code = new ContentDetails(false, settings.groupid_code, 0x0L, settings.fstflags_code);
            rules.CreateNewRule("/code/app.xml", common_details_code);
            rules.CreateNewRule("/code/cos.xml", common_details_code);
            ContentDetails common_details_meta = new ContentDetails(true, settings.groupid_meta, 0x0L, settings.fstflags_meta);
            rules.CreateNewRule("/meta/meta.xml", common_details_meta);

            
            common_details_meta = new ContentDetails(true, settings.groupid_meta, 0x0L, settings.fstflags_meta);
            rules.CreateNewRule("/meta/.*[^.xml)]+", common_details_meta);

           
            common_details_meta = new ContentDetails(true, settings.groupid_meta, 0x0L, settings.fstflags_meta);
            rules.CreateNewRule("/meta/bootMovie.h264", common_details_meta);
           
            rules.CreateNewRule("/meta/bootLogoTex.tga", common_details_meta);

           
            ContentDetails common_details_meta_manual = new ContentDetails(true, settings.groupid_meta, 0x0L, settings.fstflags_meta);
            rules.CreateNewRule("/meta/Manual.bfma", common_details_meta_manual);

      
            ContentDetails common_details_meta_images = new ContentDetails(true, settings.groupid_meta, 0x0L, settings.fstflags_meta);
            rules.CreateNewRule("/meta/.*.jpg", common_details_meta_images);

           
            rules.CreateNewRule("/code/.*(.rpx|.rpl)", common_details_code, true); 

            ContentDetails common_details_preload = new ContentDetails(true, settings.groupid_code, 0x0L, settings.fstflags_code);
            rules.CreateNewRule("/code/preload.txt", common_details_preload); 

    
            rules.CreateNewRule("/code/fw.img", common_details_code);
      
            rules.CreateNewRule("/code/fw.tmd", common_details_code);
          
            rules.CreateNewRule("/code/htk.bin", common_details_code);

            rules.CreateNewRule("/code/rvlt.tik", common_details_code);

            rules.CreateNewRule("/code/rvlt.tmd", common_details_code);

          
            ContentDetails common_details_content = new ContentDetails(true, contentGroup, titleID, settings.fstflags_content);
            rules.CreateNewRule("/content/.*", common_details_content);
            return rules;
        }
    }
}
