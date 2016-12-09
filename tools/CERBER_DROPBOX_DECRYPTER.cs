using System;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{

    // DROPBOX API 2.0 CERBER FILE RESTORE
    // PHILIPPE-ANTOINE PLANTE
    // 2016-12-08
    // THIS PROGRAM REMOVES .hta README files and encryped Files. It restores files based on file history revision. 

    class Program
    {
        // The ceber encrypted files extension ".8647" “.ba99”, ”.98a0", ".a37b", ".a563"
        private  const string EncryptedFileExtension = ".8647";
        // File types 
        private const string EncryptedFilesTypesRegex  = @"\w+\.(gif|groups|hdd|hpp|log|m2ts|m4p|mkv|mpeg|ndf|nvram|ogg|ost|pab|pdb|pif|png|qed|qcow|qcow2|rvt|st7|stm|vbox|vdi|vhd|vhdx|vmdk|vmsd|vmx|vmxf|3fr|3pr|ab4|accde|accdr|accdt|ach|acr|adb|ads|agdl|ait|apj|asm|awg|back|backup|backupdb|bay|bdb|bgt|bik|bpw|cdr3|cdr4|cdr5|cdr6|cdrw|ce1|ce2|cib|craw|crw|csh|csl|db_journal|dc2|dcs|ddoc|ddrw|der|des|dgc|djvu|dng|drf|dxg|eml|erbsql|erf|exf|ffd|fh|fhd|gray|grey|gry|hbk|ibd|ibz|iiq|incpas|jpe|kc2|kdbx|kdc|kpdx|lua|mdc|mef|mfw|mmw|mny|mrw|myd|ndd|nef|nk2|nop|nrw|ns2|ns3|ns4|nwb|nx2|nxl|nyf|odb|odf|odg|odm|orf|otg|oth|otp|ots|ott|p12|p7b|p7c|pdd|pem|plus_muhd|plc|pot|pptx|psafe3|py|qba|qbr|qbw|qbx|qby|raf|rat|raw|rdb|rwl|rwz|s3db|sd0|sda|sdf|sqlite|sqlite3|sqlitedb|sr2|srf|srw|st5|st8|std|sti|stw|stx|sxd|sxg|sxi|sxm|tex|wallet|wb2|wpd|x11|x3f|xis|ycbcra|yuv|contact|dbx|doc|docx|jnt|jpg|msg|oab|ods|pdf|pps|ppsm|ppt|pptm|prf|pst|rar|rtf|txt|wab|xls|xlsx|xml|zip|1cd|3ds|3g2|3gp|7z|7zip|accdb|aoi|asf|asp|aspx|asx|avi|bak|cer|cfg|class|config|css|csv|db|dds|dwg|dxf|flf|flv|html|idx|js|key|kwm|laccdb|ldf|lit|m3u|mbx|md|mdf|mid|mlb|mov|mp3|mp4|mpg|obj|odt|pages|php|psd|pwm|rm|safe|sav|save|sql|srt|swf|thm|vob|wav|wma|wmv|xlsb|3dm|aac|ai|arw|c|cdr|cls|cpi|cpp|cs|db3|docm|dot|dotm|dotx|drw|dxb|eps|fla|flac|fxg|java|m|m4v|max|mdb|pcd|pct|pl|potm|potx|ppam|ppsm|ppsx|pptm|ps|r3d|rw2|sldm|sldx|svg|tga|wps|xla|xlam|xlm|xlr|xlsm|xlt|xltm|xltx|xlw|act|adp|al|bkp|blend|cdf|cdx|cgm|cr2|crt|dac|dbf|dcr|ddd|design|dtd|fdb|fff|fpx|h|iif|indd|jpeg|mos|nd|nsd|nsf|nsg|nsh|odc|odp|oil|pas|pat|pef|pfx|ptx|qbb|qbm|sas7bdat|say|st4|st6|stc|sxc|sxw|tlg|wad|xlk|aiff|bin|bmp|cmt|dat|dit|edb|flvv)$";
        // Your encrypted dropbox API KEY 
        private const string DropBoxAPIKey = "";
        private static int RestoreCounter = 0;

        static void Main(string[] args)
        {
            var task = Task.Run((Func<Task>)Program.Run);
            task.Wait();
        }

        static async Task Run()
        {
            using (var dbx = new DropboxClient(DropBoxAPIKey))
            {
                
                var regex = new Regex(EncryptedFilesTypesRegex);

                var list = await dbx.Files.ListFolderAsync(string.Empty, true,true,true,true);
                do
                {
                   
                    foreach (var item in list.Entries.Where(i => i.IsFile && (i.Name.EndsWith(".8647") || i.Name.EndsWith("_.hta"))))
                    {
                        await dbx.Files.DeleteAsync(item.PathDisplay);
                    }

                    foreach (var item in list.Entries.Where(i => i.IsDeleted && !(i.Name.EndsWith(".8647") || i.Name.EndsWith("_.hta"))))
                    {
                        if (regex.IsMatch(item.Name))
                        {
                                var revisions = await dbx.Files.ListRevisionsAsync(item.PathDisplay);

                        if (revisions.Entries.Any())
                            {
                                Console.WriteLine("restoring {0} ",item.Name);
                                await dbx.Files.RestoreAsync(item.PathDisplay, revisions.Entries.Last().AsFile.Rev);
                                RestoreCounter++;
                            }
                        }       
                    }
                    list = await dbx.Files.ListFolderContinueAsync(list.Cursor);
                }
                while (list.HasMore);
                Console.WriteLine("Restore completed, press anykey to close.");
                Console.WriteLine("Restored {0} files.", RestoreCounter);
                Console.ReadKey();
            }
        }


    }
}
