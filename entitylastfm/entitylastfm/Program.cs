using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using LiteDB;

namespace entitylastfm
{

    public class Artist
    {
        public string Name { get; set; }
        public List<string> TopTracks { get; set; }
        public List<string> Genres { get; set; }
        public List<string> TopAlbums { get; set; }
        public List<string> SimilarArtists { get; set; }
        public string Bio { get; set; }
    }
    class Program
    {
        //функция добавления артиста в БД
        public static void InsertArtistIntoDb(Artist artist)
        {
            using (var db = new LiteDatabase("MyDb.db"))
            {
                var artistCollection = db.GetCollection<Artist>("artists");

                artistCollection.Insert(artist);
                artistCollection.Update(artist);
            }
        }

        public static Artist SearchArtistInDb(string art)
        {
            using (var db = new LiteDatabase("MyDb.db"))
            {
                var col = db.GetCollection<Artist>("artists");
                // Поиск по БД, где поле name, содержит введенную строку
                var searchartist = col.FindOne(x => x.Name.Contains(art));
                return searchartist;
            }

        }

        //функция вывода полей артиста в консоль
        public static void PrintArtist(Artist artist)
        {
            Console.WriteLine("Band:" + artist.Name);
            Console.WriteLine("\n\nTop Tracks:");
            foreach (var c in artist.TopTracks)
            {
                Console.WriteLine("*" + c);
            }
            Console.WriteLine("\n\nGenres:");
            foreach (var c in artist.Genres)
            {
                Console.Write(c + " ");
            }
            Console.WriteLine("\n\nTop Albums:");
            foreach (var c in artist.TopAlbums)
            {
                Console.WriteLine("*" + c);
            }
            Console.WriteLine("\n\nSimilar Artists:");
            foreach (var c in artist.SimilarArtists)
            {
                Console.WriteLine("*" + c);
            }
            Console.WriteLine("\n\nBio:" + artist.Bio);
        }

        //функция получения информации об артисте из сети
        public static Artist GetArtistInfo(string artist)
        {
            Console.Clear();
            string htmlName = "http://www.last.fm/music/" + artist;
            HtmlDocument document = null;

            try
            {
                HtmlWeb web = new HtmlWeb();
                document = web.Load(htmlName);
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("artist doesn't exist");
                Console.WriteLine("press any key to continue");
                Console.ReadLine();
                return null;
            }

            try
            {

                //Название группы
                string bandName = document.DocumentNode.SelectSingleNode("//div/h1[@class='header-title']").InnerText.Trim();

                //Популярные песни
                List<string> topTracks = new List<string>();
                HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//span[@class='chartlist-ellipsis-wrap']");

                foreach (var node in nodes)
                {
                    var topTrack = node.SelectSingleNode(".//a").Attributes["title"].Value;
                    topTracks.Add(topTrack);
                }
                //Жанры
                List<string> genres = new List<string>();
                nodes = document.DocumentNode.SelectNodes("//li[@class='tag']");

                foreach (var node in nodes)
                {
                    var genre = node.SelectSingleNode(".//a").InnerText;
                    genres.Add(genre);
                }
                //Популярные альбомы
                List<string> topAlbums = new List<string>();
                nodes = document.DocumentNode.SelectNodes(".//section[@class='grid-items-section section-with-control']//p[@class='grid-items-item-main-text']");

                foreach (var node in nodes)
                {
                    var topAlbum = node.SelectSingleNode(".//a").InnerText;
                    topAlbums.Add(topAlbum);

                }
                //Похожие исполнители
                List<string> similarArtists = new List<string>();
                nodes = document.DocumentNode.SelectNodes(".//section[@class='grid-items-section']//p[@class='grid-items-item-main-text']");

                foreach (var node in nodes)
                {
                    var similarArtist = node.SelectSingleNode(".//a").InnerText;
                    similarArtists.Add(similarArtist);

                }
                //Краткая информация о группе
                string bandInfo = document.DocumentNode.SelectSingleNode("//div[@class='wiki-content']//p").InnerText;

                return new Artist()
                {

                    Name = bandName,
                    TopTracks = topTracks,
                    Genres = genres,
                    TopAlbums = topAlbums,
                    SimilarArtists = similarArtists,
                    Bio = bandInfo

                };
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("\nno more info");
                Console.WriteLine("press any key to continue");
                Console.ReadLine();
                return null;
            }
        }



        //Функция получения информации об артисте из сети по ключевому слову
        public static Artist GetKeyWordInfo(string keyword)
        {
            string firstMatch;
            Console.Clear();
            string htmlName = "http://www.last.fm/search?q=" + keyword;
            HtmlDocument document = null;
            try
            {
                HtmlWeb web = new HtmlWeb();
                document = web.Load(htmlName);
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("\nошибка WebException\n");
            }
            try
            {
                //Название группы, первое в списке совпадений по поиску
                firstMatch = document.DocumentNode.SelectSingleNode(".//p[@class='grid-items-item-main-text']//a").InnerText.Trim();

            }
            catch (System.NullReferenceException ex1)
            {

                Console.WriteLine("no search results");
                Console.WriteLine("press any key to continue");
                Console.ReadLine();
                return null;
            }
            return GetArtistInfo(firstMatch);
        }

        public static void AddBandInEntityDB(Artist artist)
        {
            using (var db = new Context())
            {
                var genre = db.Genres.Find(artist.Genres[0]);
                if (genre == null)
                {
                    genre = new Genre()
                    {
                        Name = artist.Genres[0]
                    };
                }

                var band = new Band()
                {
                    Name = artist.Name,
                    Bio = artist.Bio,
                    MainGenre = genre
                };

                db.Bands.Add(band);
                db.SaveChanges();
            }
        }

        public static List<Band> GetBandsByGenre(string genre)
        {
            using (var db = new Context())
            {
                return
                   db.Bands.Where(x => x.MainGenre.Name.ToLower() == genre.ToLower()).ToList();
            }
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1.Search by full Artist name");
                Console.WriteLine("2.Search by keyword(first match)");
                Console.WriteLine("3.Search by Main Genre (entity)");
                ConsoleKeyInfo cki = Console.ReadKey(true);
                int choice;
                try
                {
                    choice = int.Parse(cki.KeyChar.ToString());

                    switch (choice)
                    {
                        case 1:
                            Console.Clear();
                            Console.WriteLine("Enter the artist you interested:");
                            string artist = Console.ReadLine().Replace(' ', '+');
                            var artistInput = SearchArtistInDb(artist);
                            //Если артиста нет в локальной БД, вызываем функцию получения из сети и добавляем его в БД
                            if (artistInput == null)
                            {
                                artistInput = GetArtistInfo(artist);
                                if (artistInput != null)
                                {
                                    AddBandInEntityDB(artistInput);
                                    PrintArtist(artistInput);
                                    InsertArtistIntoDb(artistInput);
                                }

                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Info from DB");
                            }
                            PrintArtist(artistInput);
                            Console.ReadLine();
                            break;
                        case 2:
                            Console.Clear();
                            Console.WriteLine("Enter the keyword to search:");
                            string keyword = Console.ReadLine().Replace(' ', '+');
                            var keywordInput = SearchArtistInDb(keyword);
                            //Если артиста нет в локальной БД, вызываем функцию получения из сети и добавляем его в БД
                            if (keywordInput == null)
                            {
                                keywordInput = GetKeyWordInfo(keyword);
                                if (keywordInput != null)
                                {
                                    PrintArtist(keywordInput);
                                    InsertArtistIntoDb(keywordInput);
                                    AddBandInEntityDB(keywordInput);
                                }

                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Info from DB");
                            }
                            PrintArtist(keywordInput);
                            Console.ReadLine();
                            break;
                        case 3:
                            Console.Clear();
                            Console.WriteLine("Enter the genre to search:");
                            string genre = Console.ReadLine();
                            var bands = GetBandsByGenre(genre);
                            Console.Clear();
                            Console.WriteLine(genre+"bands:");
                            foreach (var band in bands)
                            {
                                Console.WriteLine("*"+band.Name);
                            }
                            Console.ReadLine();
                            break;
                        default:
                            break;
                    }
                }

                catch (System.Exception exe)
                {
                    Console.ReadLine();
                    continue;

                }
            }
        }
    }
}
