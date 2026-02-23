using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Xirface
{
    public class Actions
    {
        public enum TriggerType
        {
            Clicked, Canceled, Pressed, Entered, Hovering, Exited
        }

        public void Subscribe(TriggerType type, Root root, EventHandler action)
        {
            switch (type)
            {
                case TriggerType.Clicked:
                    root.Clicked += action;
                    break;
                case TriggerType.Canceled:
                    root.Canceled += action;
                    break;
                case TriggerType.Pressed:
                    root.Pressed += action;
                    break;
                case TriggerType.Entered:
                    root.Entered += action;
                    break;
                case TriggerType.Hovering:
                    root.Hovering += action;
                    break;
                case TriggerType.Exited:
                    root.Exited += action;
                    break;
            }
        }
        public Actions(Interface Interface)
        {
            Subscribe(TriggerType.Clicked, Interface.IdentifierDict["Lines_Button"], (sender, e) => {
                Frame LinesButton = Interface.IdentifierDict["Lines_Button"] as Frame;
                Root LinesButtonTexture = Interface.IdentifierDict["Lines_Button_Texture"];
                Root LinesButtonDivider = Interface.IdentifierDict["Lines_Button_Divider"];
                Root LinesButtonText = Interface.IdentifierDict["Lines_Button_Text"];

                LinesButton.Clicked = true;

                LinesButton.FillColor = new Color(64, 64, 64, 255);
                LinesButtonTexture.FillColor = new Color(255, 255, 255, 255);
                LinesButtonDivider.FillColor = new Color(255, 255, 255, 255);
                LinesButtonText.FillColor = new Color(255, 255, 255, 255);

                LinesButton.Refresh();
                LinesButtonTexture.Refresh();
                LinesButtonDivider.Refresh();
                LinesButtonText.Refresh();

                Frame TrainsButton = Interface.IdentifierDict["Trains_Button"] as Frame;
                Root TrainsButtonTexture = Interface.IdentifierDict["Trains_Button_Texture"];
                Root TrainsButtonDivider = Interface.IdentifierDict["Trains_Button_Divider"];
                Root TrainsButtonText = Interface.IdentifierDict["Trains_Button_Text"];

                TrainsButton.Clicked = false;

                TrainsButton.FillColor = new Color(0, 0, 0, 0);
                TrainsButtonTexture.FillColor = new Color(174, 174, 174, 255);
                TrainsButtonDivider.FillColor = new Color(174, 174, 174, 255);
                TrainsButtonText.FillColor = new Color(174, 174, 174, 255);

                TrainsButton.Refresh();
                TrainsButtonTexture.Refresh();
                TrainsButtonDivider.Refresh();
                TrainsButtonText.Refresh();
            });

            Subscribe(TriggerType.Clicked, Interface.IdentifierDict["Trains_Button"], (sender, e) => {
                Frame TrainsButton = Interface.IdentifierDict["Trains_Button"] as Frame;
                Root TrainsButtonTexture = Interface.IdentifierDict["Trains_Button_Texture"];
                Root TrainsButtonDivider = Interface.IdentifierDict["Trains_Button_Divider"];
                Root TrainsButtonText = Interface.IdentifierDict["Trains_Button_Text"];

                TrainsButton.Clicked = true;

                TrainsButton.FillColor = new Color(64, 64, 64, 255);
                TrainsButtonTexture.FillColor = new Color(255, 255, 255, 255);
                TrainsButtonDivider.FillColor = new Color(255, 255, 255, 255);
                TrainsButtonText.FillColor = new Color(255, 255, 255, 255);

                TrainsButton.Refresh();
                TrainsButtonTexture.Refresh();
                TrainsButtonDivider.Refresh();
                TrainsButtonText.Refresh();

                Frame LinesButton = Interface.IdentifierDict["Lines_Button"] as Frame;
                Root LinesButtonTexture = Interface.IdentifierDict["Lines_Button_Texture"];
                Root LinesButtonDivider = Interface.IdentifierDict["Lines_Button_Divider"];
                Root LinesButtonText = Interface.IdentifierDict["Lines_Button_Text"];

                LinesButton.Clicked = false;

                LinesButton.FillColor = new Color(0, 0, 0, 0);
                LinesButtonTexture.FillColor = new Color(174, 174, 174, 255);
                LinesButtonDivider.FillColor = new Color(174, 174, 174, 255);
                LinesButtonText.FillColor = new Color(174, 174, 174, 255);

                LinesButton.Refresh();
                LinesButtonTexture.Refresh();
                LinesButtonDivider.Refresh();
                LinesButtonText.Refresh();
            });

            Subscribe(TriggerType.Clicked, Interface.IdentifierDict["Search_TextBox"], (sender, e) => {
                Frame SearchTextBox = Interface.IdentifierDict["Search_TextBox"] as Frame;
                TextBox SearchText = Interface.IdentifierDict["Search_Text"] as TextBox;

                Interface.Cursor.Focus(SearchText);
                SearchText.Refresh();
            });

            Subscribe(TriggerType.Clicked, Interface.IdentifierDict["Search_Text"], (sender, e) => {
                Root root = sender as Root;
                Interface.Cursor.Focus(root as TextBox);
                (root as TextBox).Refresh();
            });

            Subscribe(TriggerType.Clicked, Interface.IdentifierDict["Name"], (sender, e) => {
                Root root = sender as Root;
                Interface.Cursor.Focus(root as TextBox);
                (root as TextBox).Refresh();
            });

            Subscribe(TriggerType.Clicked, Interface.IdentifierDict["Prefix"], (sender, e) => {
                Root root = sender as Root;
                Interface.Cursor.Focus(root as TextBox);
                (root as TextBox).Refresh();
            });

            Subscribe(TriggerType.Entered, Interface.IdentifierDict["Lines_Button"], (sender, e) => {
                Frame LinesButton = Interface.IdentifierDict["Lines_Button"] as Frame;

                if (!LinesButton.Clicked)
                {
                    LinesButton.FillColor = new Color(64, 64, 64, 255);
                    LinesButton.Refresh();
                }
            });

            Subscribe(TriggerType.Exited, Interface.IdentifierDict["Lines_Button"], (sender, e) => {
                Frame LinesButton = Interface.IdentifierDict["Lines_Button"] as Frame;

                if (!LinesButton.Clicked)
                {
                    LinesButton.FillColor = new Color(0, 0, 0, 0);
                    LinesButton.Refresh();
                }
            });

            Subscribe(TriggerType.Entered, Interface.IdentifierDict["Trains_Button"], (sender, e) => {
                Frame TrainsButton = Interface.IdentifierDict["Trains_Button"] as Frame;

                if (!TrainsButton.Clicked)
                {
                    TrainsButton.FillColor = new Color(64, 64, 64, 255);
                    TrainsButton.Refresh();
                }
            });

            Subscribe(TriggerType.Exited, Interface.IdentifierDict["Trains_Button"], (sender, e) => {
                Frame TrainsButton = Interface.IdentifierDict["Trains_Button"] as Frame;

                if (!TrainsButton.Clicked)
                {
                    TrainsButton.FillColor = new Color(0, 0, 0, 0);
                    TrainsButton.Refresh();
                }
            });
        }

        public void Zoom(Interface Interface)
        {
            Text ZoomText = Interface.IdentifierDict["Zoom_Text"] as Text;
            ZoomText.Content = ((int)(100)).ToString() + "%";
            ZoomText.Refresh();
        }
    }
}