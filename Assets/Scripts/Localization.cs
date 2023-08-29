using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//0 English
//1 Hrvatski

namespace Localization
{
    public static class Loc
    {
        public static string loc(Languages lang)
        {
            switch(lang)
            {
                case Languages.English: return "English";
                case Languages.Hrvatski: return "Croatian";
            }

            return "";
        }

        public static string[] loc()
        {
            string[] s = { "English", "Hrvatski" };
            return s;
        }

        public static string loc(string[] txt)
        {
            return txt[(int)Settings.GameLanguage];
        }

        public static void locObj(Transform tf, string[] txt) 
        {
            locObj(tf.gameObject, txt);
        }

        public static void locObj(GameObject obj, string[] txt)
        {
            obj.GetComponent<TMPro.TMP_Text>().text = loc(txt);
        }

        public static void locObj(Button obj, string[] txt)
        {
            var comp = obj.gameObject.GetComponent<TMPro.TMP_Text>();
            if(comp == null) 
                Debug.Log(string.Format("{0} doesnt have text", obj.gameObject.name));
            else comp.text = loc(txt);
        }

        public static void locChild(GameObject parent, string[] txt)
        {
            var comp = parent.GetComponentInChildren<TMPro.TMP_Text>();
            if(comp == null) 
                Debug.Log(string.Format("{0} doesnt have text in child", parent.name));

            parent.GetComponentInChildren<TMPro.TMP_Text>().text = loc(txt);
        }

        public static void locChild(Button parent, string[] txt)
        {
            parent.gameObject.GetComponentInChildren<TMPro.TMP_Text>().text = loc(txt);
        }
    }

    public static class MainMenuTxt
    {
        public static string[] Title =
        {
            "InsideOut",
            "InsideOut"
        };

        public static string[] Play =
        {
            "Play",
            "Igraj"
        };

        public static string[] Quiz =
        {
            "Quiz",
            "Kviz"
        };

        public static string[] Volume =
        {
            "Volume",
            "Glasnoća"
        };        
        
        public static string[] Back =
        {
            "Back",
            "Povratak"
        };

        public static string[] Language =
        {
            "Language",
            "Jezik"
        };

        public static string[] Settings =
        {
            "Settings",
            "Postavke"
        };

        public static string[] Exit =
        {
            "Exit",
            "Izlaz"
        };

        public static string[] Credits =
        {
            "Credits",
            "Zasluge"
        };

        public static string[] CreditsText =
        {
            @"Students:
Bruno Perković

Mentors:
Mirko Sužnjević",
            @"Studenti:
Bruno Perković

Mentori:
Mirko Sužnjević"
        };
    }

    public static class StoryTxt
    {
        public static string[] ComputerResources =
        {
            "Computer Resources",
            "Računalni resursi"
        };

        public static string[] UserInput =
        {
            "User Input",
            "Korisničko sučelje"
        };

        public static string[] Output =
        {
            "Output",
            "Izlaz"
        };

        public static string[] ExitToMainMenu =
        {
            "Exit to Main Menu?",
            "Povratak na glavni izbornik?"
        };

        public static string[] Congrats =
        {
            @"Congratulations, you have successfuly built your PC.
If you want to play some games on it, you can press the red button.",
            @"Čestitamo, uspješno ste izgradili svoje računalo.
Ako želite igrati neke igre na njemu, stisnite crveni gumb."
        };

        public static string[] Mouse =
        {
            "Mouse",
            "Miš"
        };

        public static string[] Keyboard =
        {
            "Keyboard",
            "Tipkovnica"
        };
        
        public static string[] RedController =
        {
            "Red Controller",
            "Crveni kontroler"
        };
        
        public static string[] BlueController =
        {
            "Blue Controller",
            "Plavi kontroler"
        };
        
        public static string[] Display =
        {
            "Display",
            "Prikaz"
        };

        public static string[] Simulation =
        {
            "Simulation",
            "Simulacija"
        };

        public static string[] Network =
        {
            "Network",
            "Mreža"
        };

        public static string[] Memory =
        {
            "Memory",
            "Memorija"
        };

        public static string[] Rasterization =
        {
            "Rasterization",
            "Rasterizacija"
        };

        public static string[] Electricity =
        {
            "Electricity",
            "Struja"
        };

        public static string[] QuitPuzzle =
        {
            "Quit puzzle?",
            "Odustani od puzle?"
        };

        public static string[] Error =
        {
            "Error!\nSolve a puzzle to fix it",
            "Kvar!\nRiješi puzlu da bi ga popravio"
        };

        public static string[] Completed =
        {
            "You have successfully solved the puzzle!",
            "Uspješno ste odradili puzlu!"
        };
    }
}