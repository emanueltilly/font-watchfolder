using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Drawing;

namespace Font_Watchfolder
{

    public class FontHandling
    {
        [DllImport("gdi32", EntryPoint = "AddFontResource")]
        public static extern int AddFontResourceA(string lpFileName);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int CreateScalableFontResource(uint fdwHidden, string
            lpszFontRes, string lpszFontFile, string lpszCurrentPath);


        public static bool RegisterFont(string fullPathToFont)
        {
            //Get font name
            string contentFontName = Path.GetFileName(fullPathToFont);
            // Creates the full path where your font will be installed
            var fontDestination = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), contentFontName);

            if (!File.Exists(fontDestination))
            {
                Console.WriteLine("Installing font " + contentFontName + "...");

                string actualFontName = string.Empty;

                try
                {
                    // Copies font to destination
                    System.IO.File.Copy(fullPathToFont, fontDestination);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(fullPathToFont);
                    Console.WriteLine(fontDestination);
                    Console.WriteLine("\nERROR COPYING NEW FONT TO DESTINATION - " + contentFontName + "\n" + ex.Message);
                    return false;
                }

                try
                {
                    // Retrieves font name
                    // Makes sure you reference System.Drawing
                    PrivateFontCollection fontCol = new PrivateFontCollection();
                    fontCol.AddFontFile(fontDestination);
                    actualFontName = fontCol.Families[0].Name;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nERROR CREATING PRIVATE FONT COLLECTION -" + contentFontName + "\n" + ex.Message);
                    return false;
                }


                try
                {
                    //Add font
                    AddFontResource(fontDestination);
                    //Add registry entry   
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", actualFontName, contentFontName, RegistryValueKind.String);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nERROR ADDING FONT TO REGISTRY -" + contentFontName + "\n" + ex.Message);
                    return false;
                }

                Console.WriteLine("Success!\n");
                return true;


            }
            else
            {
                Console.WriteLine("Font already in local font folder, but not installed: " + contentFontName);
                return false;
            }
        }

        public static List<fontfile> GetInstalledFonts()
        {
            using (InstalledFontCollection fontsCollection = new InstalledFontCollection())
            {
                FontFamily[] fontFamilies = fontsCollection.Families;
                List<fontfile> fonts = new List<fontfile>();
                foreach (FontFamily font in fontFamilies)
                {
                    fonts.Add(new fontfile() { filepath = null, name = font.Name });
                }
                return fonts;
            }
        }

        public static List<fontfile> Compare(string watchfolderPath)
        {
            string localFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts);

            List<fontfile> installedFonts = GetInstalledFonts();
            List<fontfile> missingFonts = new List<fontfile>();

            foreach (var font in Directory.GetFiles(watchfolderPath, "*.*", SearchOption.AllDirectories))
            {
                //Skip non-font files
                if (!FileIsFont(font)) { continue; }

                //Generate name, skip if not able to get name
                string tmpName = GetFontNameFromFile(font);
                if (tmpName == null) { continue; }

                //Create new font object
                fontfile currentFont = new fontfile()
                {
                    filepath = font,
                    name = tmpName
                };

                //Compare font to already installed fonts
                if (installedFonts.Any(f => f.name == currentFont.name)) { continue; }

                //Add to list of missing fonts
                missingFonts.Add(currentFont);


            }
            if (missingFonts.Count > 0)
            {
                return missingFonts;
            }
            else
            {
                return null;
            }
        }

        private static bool FileIsFont(string file)
        {
            try
            {
                
                //Check if filename starts with a dot
                if (Path.GetFileName(file).Substring(0, 1) == ".") { return false; }

                List<string> allowedFiletypes = new List<string>();

                allowedFiletypes.Add(".ttf");
                allowedFiletypes.Add(".ttc");
                allowedFiletypes.Add(".otf");
                allowedFiletypes.Add(".pfb");
                allowedFiletypes.Add(".pfm");
                allowedFiletypes.Add(".fon");

                file = Path.GetExtension(file);
                if (allowedFiletypes.Contains(file.ToLower())) { return true; } else { return false; }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR FILTERING FILE AS FONT - " + file + "\n" + ex.Message);
                return false;
            }

        }

        private static string GetFontNameFromFile(string file)
        {

            try
            {
                PrivateFontCollection fontCol = new PrivateFontCollection();
                fontCol.AddFontFile(file);
                return (fontCol.Families[0].Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GETTING FONT NAME FROM FILE - " + file + "\n" + ex.Message);
                return null;
            }

        }
    }

    public class fontfile
    {
        public string name { get; set; }
        public string filepath { get; set; }
    }

}
