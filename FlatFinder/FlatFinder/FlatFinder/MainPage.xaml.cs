using Android.Webkit;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace FlatFinder
{
    struct FlatData
    {
        public List<string> Photos;
        public string ExtraInfo;
    }

    public partial class MainPage : CarouselPage
    {
        public MainPage()
        {
            InitializeComponent();
            SLpage1.BackgroundColor = Color.FromHex("C5E1A5");
            Interesting.BackgroundColor = Color.FromHex("C5E1A5");
            Find.BackgroundColor = Color.FromHex("7CB342");
            Logo.BackgroundColor = Color.FromHex("7CB342");
            Type.Items.Add("Сдаю");
            Type.Items.Add("Сдаю на сутки");
            Type.Items.Add("Продам");
            Rooms.Items.Add("1");
            Rooms.Items.Add("2");
            Rooms.Items.Add("3");
            Rooms.Items.Add("4");
            Rooms.Items.Add("5+");
            District.Items.Add("Все");
            District.Items.Add("Центральный");
            District.Items.Add("Советский");
            District.Items.Add("Первомайский");
            District.Items.Add("Партизанский");
            District.Items.Add("Заводской");
            District.Items.Add("Ленинский");
            District.Items.Add("Октябрьский");
            District.Items.Add("Московский");
            District.Items.Add("Фрунзенский");

        }

        static string getHTML(string uri)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.Default.GetString(buf, 0, count));
                }
            }
            while (count > 0);
            return sb.ToString();
        }

        //https://neagent.by/board/minsk/?catid=1&district=9&roomCount=2&priceMin=200&priceMax=300&currency=2
        private void Find_click(object sender, EventArgs e)
        {
            string URLs = CreateSearchUrl(Type.SelectedItem.ToString(), Rooms.SelectedItem.ToString(), District.SelectedItem.ToString(), PriceA.Text, PriceB.Text);
            string s = getHTML(URLs);
            
            Parse(s);
        }

        void Parse(string HTML)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(HTML);
            List<string> Street = new List<string>(100);
            List<string> AddStreet = new List<string>(100);
            List<string> price = new List<string>(100);
            List<string> text1 = new List<string>(100);
            List<string> text2 = new List<string>(100);
            List<string> URLs = new List<string>(100);

            var cells = document.QuerySelectorAll("div.imd_mid div.md_head em");
            foreach (var cell in cells)
            {
                Street.Add(cell.TextContent);
            }
            cells = document.QuerySelectorAll("div.imd_mid div.md_head em img");
            foreach (var cell in cells)
            {
                AddStreet.Add(cell.GetAttribute("src"));
            }
            for (int i = 0, j = 0; i < Street.Count; i++)
            {
                if (Street[i] == "")
                    Street[i] = AddStreet[j++];
            }
            cells = document.QuerySelectorAll("div.imd_bpic div.itm_price img");
            foreach (var cell in cells)
            {
                price.Add(cell.GetAttribute("src"));
            }
            cells = document.QuerySelectorAll("div.imd_bot a");
            foreach (var cell in cells)
            {
                if(cell.GetAttribute("href") != "javascript:void();")
                   URLs.Add(cell.GetAttribute("href"));
            }
            StackLayout stack = new StackLayout();
            for (int i = 0; i < Street.Count && i < price.Count; i++)
            {
                Image imgStreet = new Image();
                Image imgPrice = new Image();
                Label StreatText = new Label();
                if(Street[i].Length<50)
                {
                    StreatText.Text = Street[i];
                    StreatText.FontAttributes = (FontAttributes)1;
                    stack.Children.Add(StreatText);
                }
                else
                {
                    string StrSrc = Street[i].Replace("data:image/gif;base64,", "");
                    byte[] ByteSrc = Convert.FromBase64String(StrSrc);
                    imgStreet.Source = ImageSource.FromStream(() => new MemoryStream(ByteSrc));
                    imgStreet.HeightRequest = 25;
                    stack.Children.Add(imgStreet);
                }
                FlatData data = ParseExtraPage(URLs[i]);
                Label ExtraInfo = new Label();
                ExtraInfo.Text = data.ExtraInfo;
                string StrPrice = price[i].Replace("data:image/gif;base64,", "");
                byte[] BytePrice = Convert.FromBase64String(StrPrice);
                imgPrice.Source = ImageSource.FromStream(() => new MemoryStream(BytePrice));
                imgPrice.BackgroundColor = Color.Blue;
                imgPrice.HeightRequest = 15;

                ListView listView = new ListView();
                List<Image> photos = new List<Image>();
                for(int j = 0; j < data.Photos.Count; j++)
                {
                    Image img = new Image();
                    img.Source = "http:" + data.Photos[j];
                    photos.Add(img);
                }
                listView.ItemsSource = photos;
                stack.Children.Add(imgPrice);
                stack.Children.Add(ExtraInfo);
                StackLayout st = new StackLayout();
                st.Orientation = (StackOrientation)1;
                Frame f = new Frame();
                for (int j = 0; j < photos.Count; j++)
                {
                    //if(j != 2)
                   //     stack.Children.Add(photos[j]);
                    st.Children.Add(photos[j]);
                }
                ScrollView a = new ScrollView();
                a.Content = st;
                a.Orientation = (ScrollOrientation)1;
                f.Content = a;
                stack.Children.Add(f);
            }
            stack.Padding = new Thickness(10,10,10,10);
            Interesting.Content = stack;
        }

        FlatData ParseExtraPage(string URL)
        {
            string Html = getHTML(URL);
            var parser = new HtmlParser();
            var document = parser.Parse(Html);
            FlatData data = new FlatData();
            List<string> Photos = new List<string>(100);
            var cells = document.QuerySelectorAll("img.simple-gallery-item");
            foreach (var cell in cells)
            {
                string s = cell.GetAttribute("src");
                Photos.Add(s);
            }
            cells = document.QuerySelectorAll("div.text-content p");
            string str = cells[0].TextContent;
            while (str.Contains("  ")) { str = str.Replace("  ", " "); }
            data.ExtraInfo = str;
            data.Photos = Photos;
            return data;
        }

        //https://neagent.by/board/minsk/?catid=1&district=9&roomCount=2&priceMin=200&priceMax=300&currency=2
        string CreateSearchUrl(string type, string roomCount, string district, string minPrice, string maxPrice)
        {
            string Type = "";
            string District = "";
            switch (type)
            {
                case "Сдаю":
                    Type = "1";
                    break;
                case "Продам":
                    Type = "13";
                    break;
                case "Сдаю на сутки":
                    Type = "11";
                    break;
                default:
                    Type = "1";
                    break;
            }
            switch (district)
            {
                case "Все":
                    District = "0";
                    break;
                case "Центральный":
                    District = "9";
                    break;
                case "Советский":
                    District = "19";
                    break;
                case "Первомайский":
                    District = "29";
                    break;
                case "Партизанский":
                    District = "39";
                    break;
                case "Заводской":
                    District = "49";
                    break;
                case "Ленинский":
                    District = "59";
                    break;
                case "Октябрьский":
                    District = "69";
                    break;
                case "Московский":
                    District = "79";
                    break;
                case "Фрунзенский":
                    District = "89";
                    break;
                default:
                    District = "0";
                    break;
            }
            return "https://neagent.by/board/minsk/?catid=" + Type + "&district=" + District + "&roomCount=" + roomCount + "&priceMin=" + minPrice + "&priceMax=" + maxPrice + "&currency=2";
        }


    }
}
