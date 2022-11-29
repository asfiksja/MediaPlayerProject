using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace MediaPlayer
{
    public partial class MainWindow : Window
    {
        //timer dzialajacy w tle
        DispatcherTimer _timer = new DispatcherTimer();

        //slowik przechowywujacy nazwy plikow i sciezki do nich
        Dictionary<string, string> fileDictionary = new Dictionary<string, string>();

        public MainWindow()
        {

            InitializeComponent();
            //timer ktory tyka przez caly czas dzialania programu
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += new EventHandler(ticktock);
            _timer.Start();


        }

        //Code behind odpowiadajacy tikowi timera
        void ticktock(object sender, EventArgs e) //Wiktor Tkaczyk
        {
            ScorePlay.Content = ($"{sliderPlay.Value:0.0}");
            MediaProgress.Value = _media.Position.TotalSeconds;
            TrackTimeLabel.Content = ($"{_media.Position.Hours:00}:{_media.Position.Minutes:00}:{_media.Position.Seconds:00}");
            if (_media.Volume == 0)
                imgMute.Source = new BitmapImage(new Uri("Resources/mute.png", UriKind.RelativeOrAbsolute));
            else
                imgMute.Source = new BitmapImage(new Uri("Resources/volume.png", UriKind.RelativeOrAbsolute));

        }

        private void Window_Loaded(object sender, RoutedEventArgs ergs) { }

        //Funkcja updatetujaca informacje o pliku (nazwa i czas trwania)
        public void _mediaMediaOpened(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            MediaProgress.Maximum = _media.NaturalDuration.TimeSpan.TotalSeconds;
            var totalDurationTime = _media.NaturalDuration.TimeSpan;
            ContentTitle.Content = $"{fileDictionary[HistoryListView.SelectedItem.ToString()]}  \t(Duration: {totalDurationTime})";

        }

        //Kliknecie przycisku Play
        private void ButtonPlay_OnClick(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            _media.Play();
            sliderPlay.Value = 1.0; //domyslnie ustawanie szybkosci 1.0
        }

        //Klikniecie przycisku Stop
        private void ButtonStop_OnClick(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            _media.Stop();
        }

        //Klikniecie przycisku Pause/Resume
        private void ButtonPause_OnClick(object sender, RoutedEventArgs e) //Hubert Jankowski
        {
            if (PauseButton.Content.Equals("Pause"))
            {
                _media.Pause();
                PauseButton.Content = "Resume";
            }
            else
            {
                _media.Play();
                PauseButton.Content = "Pause";
            }
        }


        //Otwieranie pojedynczego pliku
        private void OpenButtonClick(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*| Video Files (*.mp4) |*.mp4"; //filtry plikow
            dlg.FilterIndex = 2;
            bool? res = dlg.ShowDialog(); //wyswietla okno wyboru plikow jako modalne - nie mozna klikac na okno glowne kiedy jest otwarte
            if (res.HasValue) //wykonuje sie jesli jest wybrany plik w oknie wyboru
            {
                fileDictionary.Add(dlg.SafeFileName, dlg.FileName); //dodawanie do slownika odpowiednio - nazwy pliku i sciezki
                HistoryListView.Items.Add(dlg.SafeFileName); //wypisywanie nazwy pliku do listy
                HistoryListView.SelectedIndex = 0; //ustawanie indexu listy na zero (zeby playlista odtwarzala sie od poczatku)
                _media.MediaOpened += _mediaMediaOpened; //dodawanie filmiku
                _media.Source = new Uri(dlg.FileName); //natychmiastowe odtwarzanie filmiku po dodaniu
                _media.Play();

            }

            if (AddPlayList.Content.Equals("Add Playlist"))
            {
                AddPlayList.Content = "Change Playlist";
            }
        }

        //Klikniecie przycisku odpowiadajacego za czyszczenei playlisty
        private void ClearHistoryButtonOnClick(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            HistoryListView.Items.Clear(); //czyszczenie playlisty
            fileDictionary.Clear(); //czyszczenei slownika z nazwami i sciezkami do filmikow

        }

        //Zmienianie dzwieku filmiku sliderem
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //Wiktor Tkaczyk
        {
            ((Slider)sender).SelectionEnd = e.NewValue; //pozwala userowi zmieniac wartosc przesuwajac slider
            _media.Volume = VolumeSlider.Value; //zmienianie samej glosnosci

        }

        //Klikniecie na dany element w playliscie
        private void HistoryListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) //Wiktor Tkaczyk
        {
            DoubleClickHistory();
        }


        //tworzenie playlisty
        private void AddPlayList_OnClick(object sender, RoutedEventArgs e) //Jakub Stramski
        {
            HistoryListView.Items.Clear(); //czyszczenie playlisty bo dodajemy nowa
            fileDictionary.Clear();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*| Video Files (*.mp4) |*.mp4";
            dlg.FilterIndex = 2;
            dlg.Multiselect = true; //pozwala na wybranie wiecej niz jednogo pliku
            bool? res = dlg.ShowDialog();
            if (res.HasValue)
            {
                foreach (var itemFileName in dlg.FileNames) //kazdy wybrany plik dodajemy do slownika
                {
                    fileDictionary.Add(System.IO.Path.GetFileName(itemFileName), itemFileName);
                }

                foreach (var x in fileDictionary) //kazdy wybrany plik dodajemy do playlisty
                {
                    HistoryListView.Items.Add(x.Key);
                }

                if (AddPlayList.Content.Equals("Add Playlist"))
                {
                    AddPlayList.Content = "Change Playlist";
                }

                HistoryListView.SelectedIndex = 0; //zaczynanie playlisty od poczatku
            }
        }

        //odtwarzanie poprzedniego filmiku (jesli takowy byl)
        private void PrevButton_Click(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            if (HistoryListView.HasItems && HistoryListView.SelectedIndex >= 1)
            {
                HistoryListView.SelectedIndex--;
                DoubleClickHistory();
            }
        }

        //usuwanie wybranego filmiku z playlisty
        private void DeleteButton_OnClickButton_Click(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            HistoryListView.Items.Remove(HistoryListView.SelectedItem);
            HistoryListView.SelectedIndex = 0;
        }

        //odtwarzanie nastepnego filmiku z playlisty
        private void NextButton_OnClickButton_Click(object sender, RoutedEventArgs e) //Wiktor Tkaczyk
        {
            if (HistoryListView.HasItems)
            {
                HistoryListView.SelectedIndex++;
                DoubleClickHistory();
            }


        }


        //Jak skonczy sie filmik to odtwarzany jest nastepny w kolejce
        private void _media_OnMediaEnded(object sender, RoutedEventArgs e) //Jakub Stramski
        {
            NextButton_OnClickButton_Click(sender, e);
        }


        //Klikniecie na ProgressBar - przewijanie filmiku do danego miejsca
        private void MediaProgress_OnMouseDown(object sender, MouseButtonEventArgs e) //Jakub Stramski
        {
            double x = e.GetPosition(MediaProgress).X;
            double pos = x * 100 / MediaProgress.ActualWidth;
            _media.Position = TimeSpan.FromSeconds(_media.NaturalDuration.TimeSpan.TotalSeconds / 100.0 * pos);
        }


        //zmienianie szybkosci odtwarzania filmiku przez uzytkownika przy uzyciu slidera
        private void sliderPlay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //Jakub Stramski
        {
            _media.SpeedRatio = e.NewValue;
        }


        //Wyciszanie dzwieku
        private void MuteButton_Click(object sender, RoutedEventArgs e) //Jakub Stramski
        {
            if (_media.Volume == 0.0)
            {
                imgMute.Source = new BitmapImage(new Uri("Resources/volume.png", UriKind.RelativeOrAbsolute)); //zmienianie obrazka po kliknieciu
                VolumeSlider.IsEnabled = true;
                _media.Volume = VolumeSlider.Value;
            }

            else
            {

                imgMute.Source = new BitmapImage(new Uri("Resources/mute.png", UriKind.RelativeOrAbsolute));
                _media.Volume = 0.0;
                VolumeSlider.IsEnabled = false;
            }
        }


        //Otworzenie pliku klienietego dwa razy
        public void DoubleClickHistory() //Jakub Stramski
        {
            //po dwukrotnym klikniecu aktualnie odtwarzany plik zmieni sie na wybrany przez nas
            _media.Source = new Uri(fileDictionary[HistoryListView.SelectedItem.ToString()]); //wyszukiwanie kliknietego filmiku w playliscie
            _media.MediaOpened += _mediaMediaOpened;
            _media.Play();

        }

    }
}
