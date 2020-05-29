using Bridge;
using Newtonsoft.Json;
using System;
using TSBTool;
using Bridge.Html5;

namespace Bridge_TSBTool
{
    /// <summary>
    /// https://github.com/BAD-AL/tsbtools
    /// 
    /// To build this project I'm using Visual Studio 2019,
    ///         Bridge.NET and Bridge.Html5 were installed via nuget (Package Manager Console)
    /// 
    /// Public API 
    /// window.tecmoTool: {
    ///     bool loadFile(string input): Prompts user to load a ROM; returns true when successful
    ///     string getAllContents({options}): Get contents for the loaded ROM
    ///     void applyData(string): Applies the data passed
    ///     void saveFile(): save the current work to a rom.
    ///     
    ///     string convertToTSB1Format(string): Converts data to TSB1 Format; returns the converted data
    ///     string convertToTSB2Format(string): Converts data to TSB2 Format; returns the converted data
    ///     string convertToTSB3Format(string): Converts data to TSB3 Format; returns the converted data
    /// }
    /// </summary>
    public class App
    {
        static string sButtonHeight = "40px";
        static string sButtonMargin = "5px";
        static ITecmoTool tool = null;

        private static HTMLTextAreaElement mTextBox = null;
        private static HTMLButtonElement applyButton = null;
        private static HTMLButtonElement viewButton = null;
        private static HTMLButtonElement saveButton = null;

        public static void Main()
        {
            //RenderSamplePage();
            HTMLInputElement loadButton = new HTMLInputElement()
            {
                Id = "mHiddenLoadButton",
                Type = InputType.File,
                InnerHTML = "Load File",
                Style = { Display = Display.None }
            };
            loadButton.OnChange = (ev) => {
                LoadFile(loadButton.Files[0]);
            };
            Document.Body.AppendChild(loadButton);
        }
        /// <summary>
        /// https://github.com/bridgedotnet/Bridge/wiki
        /// </summary>
        public static void RenderSamplePage()
        {
            // After building (Ctrl + Shift + B) this project, 
            // browse to the /bin/Debug or /bin/Release folder.

            // A new bridge/ folder has been created and
            // contains your projects JavaScript files. 

            // Open the bridge/index.html file in a browser by
            // Right-Click > Open With..., then choose a
            // web browser from the list

            // This application will then run in the browser.

            mTextBox = new HTMLTextAreaElement()
            {
                Id = "mTextBox",
                Style = {
                    MarginLeft = "5px",
                    Height= "65vh",
                    Width = "92%",
                    MaxWidth  = "700px"
                }
            };
            mTextBox.SetAttribute("spellcheck", "false");
            Document.Body.AppendChild(mTextBox);
            Document.Body.AppendChild(new HTMLParagraphElement());

            HTMLInputElement loadButton = new HTMLInputElement()
            {
                Id = "mLoadButton",
                Type = InputType.File,
                InnerHTML = "Load File",
                Style = {
                    Margin = sButtonMargin, Height = sButtonHeight
                }
            };
            loadButton.OnChange = (ev) => {
                LoadFile(loadButton.Files[0]);
                // could also just use "PromptAndLoadFile();"
            };
            Document.Body.AppendChild(loadButton);

            viewButton = new HTMLButtonElement()
            {
                Id = "mViewButton",
                InnerHTML = "View Contents",
                Style = {
                    Margin = sButtonMargin, Height = sButtonHeight
                }
            };
            viewButton.OnClick = (ev) => { ViewContents(); };
            Document.Body.AppendChild(viewButton);

            applyButton = new HTMLButtonElement()
            {
                Id = "mApplyButton",
                InnerHTML = "Apply To ROM",
                Style = {
                    Margin = sButtonMargin, Height = sButtonHeight
                }
            };
            applyButton.OnClick = (ev) => { ApplyToRom(); };
            Document.Body.AppendChild(applyButton);

            saveButton = new HTMLButtonElement()
            {
                Id = "mSaveButton",
                InnerHTML = "Save File",
                Style = {
                    Margin = sButtonMargin, Height = sButtonHeight
                }
            };
            saveButton.OnClick = (ev) => { SaveFile(); };
            Document.Body.AppendChild(saveButton);
            state1();
        }


        private static void LoadROM(byte[] rom)
        {
            tool = TecmoToolFactory.GetToolForRom(rom)as ITecmoTool;
            if (tool != null && tool.OutputRom != null)
            {
                Console.WriteLine("ROM Loaded; version = " + tool.RomVersion);
                state2();
            }
            else
                state1();
        }

        private static void state1()
        {
            if (viewButton != null)
            {
                viewButton.Disabled = saveButton.Disabled = applyButton.Disabled = true;
            }
        }
        private static void state2()
        {
            if (viewButton != null)
            {
                viewButton.Disabled = saveButton.Disabled = applyButton.Disabled = false;
            }
        }

        private static void ViewContents()
        {
            mTextBox.Value = tool.GetKey() + "\n" + tool.GetAll();
        }

        private static void ApplyToRom()
        {
            Console.WriteLine("ApplyToRom() called!");
            if (tool != null)
            {
                string stuff = mTextBox.Value;
                tool.ProcessText(stuff);
            }
        }

        //These functions will appear at: Bridge_TSBTool.App.LoadFile
        /// <summary>
        /// used like: loadButton.OnChange = (ev) => { LoadFile(loadButton.Files[0]);  };
        /// Where 'loadButton' is an input element (button) 
        /// </summary>
        /// <param name="blob"></param>
        public static void LoadFile(File blob)
        {
            FileReader reader = new FileReader();

            reader.OnLoad = (ev) => {
                var arrayBuffer = reader.Result;
                Uint8Array arr = new Uint8Array(arrayBuffer);
                byte[] rom = new byte[arr.ByteLength];
                for (int i = 0; i < arr.ByteLength; i++)
                {
                    rom[i] = arr[i];
                }
                LoadROM(rom);
            };
            reader.ReadAsArrayBuffer(blob);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Public API
        public static void PromptAndLoadFile()
        {
            Document.GetElementById("mHiddenLoadButton").Click();
        }

        /// <summary>
        /// Get contents for the loaded ROM
        /// </summary>
        /// <returns></returns>
        public static string GetAllContents()
        {
            string retVal = "<no data loaded>";
            if (tool != null)
            {
                retVal = tool.GetAll();
            }
            else
            {
                Console.WriteLine("No Rom Loaded");
            }
            return retVal;
        }

        /// <summary>
        /// Applies the data passed
        /// </summary>
        /// <param name="data">The data to apply</param>
        public static void ApplyData(string data)
        {
            if( tool != null)
            {
                tool.ProcessText(data);
            }
            else
            {
                Console.WriteLine("No Rom Loaded");
            }
        }

        /// <summary>
        /// save the current work to a rom, prompts user for filename.
        /// </summary>
        public static void SaveFile()
        {
            if (tool != null)
            {
                string fileName = Window.Prompt("Save file name", "");
                //Blob blob = Script.Call<Blob>("window.createBlobFromArrayBuffer", tool.OutputRom); 
                var u8a = new Uint8Array(tool.OutputRom);
                Blob blob = Script.Call<Blob>("window.createBlobFromArrayBuffer", u8a);
                Script.Call("window.saveFile", fileName, blob);
            }
            else
            {
                Console.WriteLine("No Rom Loaded");
            }
        }

        /// <summary>
        ///  Converts data to TSB1 Format; returns the converted data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertToTSB1Format(string input)
        {
            TSBContentType type = StaticUtils.GetContentType(input);
            return TSBTool2.TecmoConverter.Convert(type, TSBContentType.TSB1, input);
        }

        /// <summary>
        /// Converts data to TSB2 Format; returns the converted data
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string ConvertToTSB2Format(string input)
        {
            TSBContentType type = StaticUtils.GetContentType(input);
            return TSBTool2.TecmoConverter.Convert(type, TSBContentType.TSB2, input);
        }

        /// <summary>
        /// Converts data to TSB3 Format; returns the converted data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertToTSB3Format(string input)
        {
            TSBContentType type = StaticUtils.GetContentType(input);
            return TSBTool2.TecmoConverter.Convert(type, TSBContentType.TSB2, input);
        }
        #endregion
        //////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}