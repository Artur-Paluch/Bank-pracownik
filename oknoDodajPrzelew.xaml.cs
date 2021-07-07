﻿using Bank;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    public partial class oknoDodajPrzelew : Window
    {
        Window okno;
        LINQBazaBankDataContext dc = new LINQBazaBankDataContext(Bank.Properties.Settings.Default.BankConnectionString);
        private List<Klienci> listaKlientow = new List<Klienci>();
        List<int> indeksy = new List<int>();
        public oknoDodajPrzelew(Window okno)
        {
            InitializeComponent();
            this.okno = okno;
           
           
                wczytajBaze("Klienci");
        }
        private List<int> znajdzZaznaczenie(String tabela) // sprawdza czy zaznaczono pojedynczy rekord, zwraca listę z indeksem
        {
            int i = 0;
            List<int> indeksy = new List<int>();


            switch (tabela)
            {
                case "Klienci":
                    {
                        Klienci temp = new Klienci();
                        foreach (var item in listaKlientow)
                        {

                            if (listaKlientow[i].zaznaczony == true)
                            {

                                indeksy.Add(listaKlientow[i].Id_klienta);
                            }


                            i++;
                        }
                        return indeksy;
                    }
             
                default:
                    {
                        return indeksy;
                    }

            }
        }

        public void wczytajBaze(string tabela)
        {

            switch (tabela)
            {
                case "Klienci":
                    {
                        dc.Refresh(RefreshMode.OverwriteCurrentValues, dc.Klienci);
                        var zapytanie = dc.ExecuteQuery<Klienci>("select [Id klienta],[aktywny],[imię] ,nazwisko,[telefon],[miasto], ulica,[środki]from [klienci] where [aktywny]=1");

                        listaKlientow.Clear();




                        foreach (Klienci item in zapytanie)
                        {
                            if (item.aktywny.Equals(true))
                            {
                                item.zaznaczony = false;
                                listaKlientow.Add(item);
                            }
                        }
                        dataGridKlienci2.ItemsSource = null;
                        dataGridKlienci2.ItemsSource = listaKlientow;
                        
                        break;
                    }

            }

        }
        private void dodaj_Click(object sender, RoutedEventArgs e)
        {
            indeksy = znajdzZaznaczenie("Klienci");
            if (indeksy.Count().Equals(0))
            {
                MessageBox.Show("Musisz wybrać klienta.");
            }
            else if (indeksy.Count().Equals(2))
            {
                MessageBox.Show("Wybierz jednego klienta.");
            }
            else
            {
                int intParse1;
                if (!int.TryParse(TextBoxRachunekOdbiorcy.Text, out intParse1) || TextBoxRachunekOdbiorcy.Text.Length > 30)
                {

                    MessageBox.Show("Błędny numer rachunku Odbiorcy. ");
                    TextBoxRachunekOdbiorcy.Text = "";
                }
                else
                {
                    decimal decimalParse1;
                    if (!decimal.TryParse(textBoxKwotaPrzelewu.Text, out decimalParse1))
                    {

                        MessageBox.Show("Błędna kwota przelewu. Musi być liczba.");
                        textBoxKwotaPrzelewu.Text = "";
                    }
                    else
                    {

                        if (decimalParse1 < 1 || textBoxKwotaPrzelewu.Text.Length > 30)
                        {
                            MessageBox.Show("Błędna kwota przelewu.");
                            textBoxKwotaPrzelewu.Text = "";
                        }
                        else
                        {
                            var KlientDoZamiany = dc.Klienci.First(p => p.Id_klienta == indeksy[0]);

                            if (KlientDoZamiany.Środki >= decimalParse1)
                            {
                                    var zapytanie =
                from c in dc.Klienci
                where c.Id_klienta == indeksy[0]
                select c;

                                DateTime tempData = DateTime.Now;


                                Przelewy bob = new Przelewy();
                                bob.Nazwa_odbiorcy = textBoxNazwaOdbiorcy.Text;
                                bob.Numer_rachunku_odbiorcy = intParse1;
                                bob.Kwota = decimalParse1;
                                bob.Tytuł_przelewu = textBoxTytulem.Text;
                                bob.Nadawca = indeksy[0];
                                bob.Data = tempData;
                                bob.zaznaczony = false;
                                dc.Przelewy.InsertOnSubmit(bob);
                                dc.SubmitChanges();

                                KlientDoZamiany.Środki = KlientDoZamiany.Środki - bob.Kwota;
                                dc.SubmitChanges();

                                MessageBox.Show("Przelew udał sie.");

                                this.Close();

                            }


                            else
                            {
                                MessageBox.Show("Za mało środków na koncie.");
                            }
                        }
                    }
                }
                
            }
            
        }

        private void anuluj_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void zaznaczonytextBoxKwotaPrzelewu(object sender, RoutedEventArgs e)
        {
            textBoxKwotaPrzelewu.Foreground = Brushes.Black;
            textBoxKwotaPrzelewu.FontWeight = FontWeights.Normal;
            textBoxKwotaPrzelewu.Text = "";
        }

        private void zaznaczonytextBoxNazwaOdbiorcy(object sender, RoutedEventArgs e)
        {
            textBoxNazwaOdbiorcy.Foreground = Brushes.Black;
            textBoxNazwaOdbiorcy.FontWeight = FontWeights.Normal;
            textBoxNazwaOdbiorcy.Text = "";
        }

        private void zaznaczonyTextBoxRachunekOdbiorcy(object sender, RoutedEventArgs e)
        {
            TextBoxRachunekOdbiorcy.Foreground = Brushes.Black;
            TextBoxRachunekOdbiorcy.FontWeight = FontWeights.Normal;
            TextBoxRachunekOdbiorcy.Text = "";
        }

        private void zaznaczonytextBoxTytulem(object sender, RoutedEventArgs e)
        {
            textBoxTytulem.Foreground = Brushes.Black;
            textBoxTytulem.FontWeight = FontWeights.Normal;
            textBoxTytulem.Text = "";
        }
        

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (null == checkBox) return;

            if (checkBox.IsChecked == true)
            {

                if (dataGridKlienci2.ItemsSource != null)
                {
                    for (int i = 0; i < listaKlientow.Count(); i++)
                    {
                        listaKlientow[i].zaznaczony = true;
                    }

                    dataGridKlienci2.ItemsSource = null;
                    dataGridKlienci2.ItemsSource = listaKlientow;
                    dataGridKlienci2.Items.Refresh();
                }

                else
                {

                    if (dataGridKlienci2.ItemsSource != null)
                    {
                        for (int i = 0; i < listaKlientow.Count(); i++)
                        {
                            listaKlientow[i].zaznaczony = false;
                        }

                        dataGridKlienci2.ItemsSource = null;
                        dataGridKlienci2.ItemsSource = listaKlientow;
                        dataGridKlienci2.Items.Refresh();
                    }


                }

            }
        }
    }
}