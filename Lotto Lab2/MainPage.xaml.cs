using System;
using System.ComponentModel;
using System.ServiceModel.Channels;
using Windows.Gaming.Input;
using Windows.Globalization.PhoneNumberFormatting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lotto_Lab2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TextBox[] fields;
        private Windows.UI.Xaml.Shapes.Rectangle[] fields_Status_recs;
        private BackgroundWorker worker;
        public MainPage()
        {
            this.InitializeComponent();

            this.fields = new TextBox[]
            {
                F1, F2, F3, F4, F5, F6, F7,
            };
            this.fields_Status_recs = new Windows.UI.Xaml.Shapes.Rectangle[]
            {
                S1, S2, S3, S4, S5, S6, S7,
            };
            BigNumGoodYupp.Text = "1";
            MainSlider.Value = 1;

        }
   
      
        private void StartRollingBtn_Click(object sender, RoutedEventArgs e)
        {
            int[] isGood = State_Refresh(); // get status for all inputs if any of the inputs is incorrect
            bool everyThingIsGood = true;
            for (int i = 0; i < isGood.Length; i++)
            {
                if (isGood[i] != 1)
                {
                    everyThingIsGood = false; break;
                }
            }
            if (everyThingIsGood)
            {
                //worker.RunWorkerAsync();
                StartRolling();
                return;
            }
            else
            {
                DisplayErrors(isGood);
            }
        }

        private void StartRolling()
        {

            int amount = (int)MainSlider.Value;
            int[] myRow = GetInputRow();

            // start the game //
            int winCount5 = 0, winCount6 = 0, winCount7 = 0;
            for (int n = 0; n < amount; n++)
            {
                int miss = 0;
                int[] winner = NewRandomRow();
                for (int i = 0; i < myRow.Length; i++)
                {
                    for (int j = 0; j < winner.Length; j++)
                    {
                        if (myRow[i] == winner[j]) { miss--; break; }

                    }
                    miss++;
                    if (miss == 3) break;
                }
                if (miss == 3) { continue; }
                else if (miss == 2) { winCount5++; }
                else if (miss == 1) { winCount6++; }
                else if (miss == 0) { winCount7++; }

            }
            Res5.Text = "Fem rätt: " + winCount5.ToString();
            winCount5 = (winCount5 == 0) ? -1 : 1;
            Color_Status_Rect(winCount5, Res5_rec);
            Res6.Text = "Sex rätt: " + winCount6.ToString();
            winCount6 = (winCount6 == 0) ? -1 : 1;
            Color_Status_Rect(winCount6, Res6_rec);
            Res7.Text = "Sju rätt: " + winCount7.ToString();
            winCount7 = (winCount7 == 0) ? -1 : 1;
            Color_Status_Rect(winCount7, Res7_rec);




        }
        private int[] GetInputRow()
        {
            int[] row = new int[fields.Length];
            for(int i = 0; i < fields.Length; i++)
            {
                int.TryParse(fields[i].Text, out row[i]);
            }
            return row;
        }

        private void DisplayErrors(int[] problems) {

            int problemCount = 0;
            for (int i = 0; i < problems.Length; i++) { if (problems[i] != 1) { problemCount++; } }
            String[] errorMsgs = new String[problemCount];
            // Advanced error message display is a waste of time for this project 
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)StartRollingBtn);
            // TODO: display errormsgs

        }
        private int[] State_Refresh()
        {
            int[] state = new int[fields.Length+1];// + check if the silider and the filed are in sync
            for (int i = 0; i < fields.Length; i++)
            {
                state[i] = Update_Rec_Status(i, fields, fields_Status_recs);
            }
            if(int.TryParse(BigNumGoodYupp.Text, out state[fields.Length])) {
                if (state[fields.Length] == (int)MainSlider.Value) { state[fields.Length] = 1; } // slider and the field are in sync (good)
                else { state[fields.Length] = -1; }// hur lyckas du forstora slideren (out of sync)
            }else { state[fields.Length] = -2; } // somthing went worng (bigNumGoodYupp should be a number)
            return state;
        }

        private void Text_Changed_FX(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int index = int.Parse(textBox.Name.ToString().Substring(1)) - 1;
            Update_Rec_Status(index, fields, fields_Status_recs);

        }
        private void State_Refresh_WhenLostFocus(object sender, RoutedEventArgs e)
        {
            State_Refresh();
        }

        private void Generate_Random_Row(object sender, RoutedEventArgs e)
        {
            int[] newRandomRow = NewRandomRow();
            for(int i = 0; i < fields.Length; i++) 
            {
                fields[i].Text = newRandomRow[i].ToString();
            }
        }

        private int Update_Rec_Status(int index, TextBox[] fields, Rectangle[] fields_Status_recs)
        {

            int num;
            if (fields[index].Text == "")
            {
                Color_Status_Rect(0, fields_Status_recs[index]);
                return 0; // empty field 

            }
            try
            {
                num = Int32.Parse(fields[index].Text);
                if (num < 1 || num > 35) { Color_Status_Rect( -1, fields_Status_recs[index]); return -1; } // out of range
            }
            catch
            {
                Color_Status_Rect(-2, fields_Status_recs[index]);
                return -2;// format excption 
            }
            // check duplicate
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Text == "" || i == index) continue;
                try
                {
                    int numIndex = int.Parse(fields[i].Text);
                    if (num == numIndex) { Color_Status_Rect(-3, fields_Status_recs[index]); return -3; }// duplicate found
                }
                catch
                {
                    continue;
                }
            }

            Color_Status_Rect(1, fields_Status_recs[index]);
            return 1;// correct formating (accepted)
        }
        private void Color_Status_Rect(int status, Rectangle rec)
        {
            string color;
            if (status < 0)
            {
                
                color = "#c00";

            }
            else if (status > 0)
            {
                color = "#0a0";
            }
            else
            {
                color = "#888";
            }
            // https://stackoverflow.com/questions/44860768/converting-string-type-to-windows-ui-color-in-windows-universal-app
            var bruch = new SolidColorBrush((Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), color));
            rec.Fill = bruch;
        }

        private int[] NewRandomRow()
        {
            
            Random random = new Random();
            int[] randoms = new int[fields.Length];
            int i;
            for(i = 0; i < fields.Length; i++) // generate a set of randoms. (might contain duplicates)
            {
                randoms[i] = random.Next(1,35);
            }
            i = 1;
            while (i < fields.Length) { // check for duplicates
                for(int j = 0; j < i; j++)
                {
                    if (randoms[i] == randoms[j]) // regenerate random number if a duplicate found
                    {
                        randoms[i] = random.Next(1, 35);
                        j = -1; 
                    }
                }
                i++;
            }
            return randoms;
        }

        private void Amount_Checks(object sender, TextChangedEventArgs e)
        {
            TextBox thisTextBox = sender as TextBox;
            String t = thisTextBox.Text;

            int num;
            for (int i = t.Length - 1; i >= 0; i--)
            {
                bool isANum = Int32.TryParse(t.Substring(i, 1), out num);
                // if one of the charectars is not a number remove it
                if (!isANum) { thisTextBox.Text = t.Substring(0, i) + t.Substring(i+1, t.Length - (i+1)); }
            }
            if(int.TryParse(t,out num))
            {
                MainSlider.Value = num;
                if (num == 0)
                    thisTextBox.Text = "1";
            }
            else
            {
                BigNumGoodYupp.Text = MainSlider.Value.ToString();
            }
                
        }

        private void MainSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            BigNumGoodYupp.Text = MainSlider.Value.ToString();
        }

    }
}
