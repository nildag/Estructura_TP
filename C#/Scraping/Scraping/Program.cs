using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;
using SeleniumExtras.WaitHelpers;

class Program
{
    static void Main(string[] args)
    {
        ChromeOptions options = new ChromeOptions();
       
        IWebDriver driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl("https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es");

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("gs_res_ccl")));

        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Titulo");
        dataTable.Columns.Add("Autor");
        dataTable.Columns.Add("Revista");
        dataTable.Columns.Add("Año");
        dataTable.Columns.Add("Editor");
        dataTable.Columns.Add("URL");
        dataTable.Columns.Add("Citado");

        IList<IWebElement> articulos = driver.FindElements(By.XPath("//*[@id='gs_res_ccl']//h3"));
        int secondsPause = 1;

        for (int i = 0; i < articulos.Count; i++)
        {
            string title = articulos[i].Text;
            string link = "";
            string citadoPor = "";
            Console.WriteLine("Titulo: "+title);
            try
            {
                IWebElement linkElement = driver.FindElement(By.XPath($"//*[@id='gs_res_ccl_mid']/div[{i+1}]/div[2]/div[3]/a[3]"));
                link = linkElement.GetAttribute("href");
                Console.WriteLine("Enlace "+link);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Error al encontrar el elemento");
            }
            

            citadoPor = articulos[i].FindElement(By.XPath($"//*[@id='gs_res_ccl_mid']/div[{i+1}]/div[2]/div[3]/a[3]")).Text;
            int numCitas = ExtractCitationCount(citadoPor);
            Console.WriteLine("Citado por: " + numCitas);

            //Interactuar con "Citar"
            IWebElement enlaceCitar = articulos[i].FindElement(By.XPath($"//*[@id='gs_res_ccl_mid']/div[{i+1}]/div[2]/div[3]/a[2]"));
            string enlaceCitarHref = enlaceCitar.GetAttribute("href");
            Console.WriteLine("Enlace Citar: " + enlaceCitarHref);

            secondsPause = new Random().Next(10, 15);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
            enlaceCitar.Click();
            //Intentar interactuar con "BibTeX"
            try
            {
                secondsPause = new Random().Next(10, 15);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                IWebElement enlaceBibTeX = driver.FindElement(By.XPath("//*[@id='gs_citi']/a[1]"));
                string enlaceBibTeXHref = enlaceBibTeX.GetAttribute("href");
                Console.WriteLine("Enlace BibTeX: " + enlaceBibTeXHref);
                enlaceBibTeX.Click();

                secondsPause = new Random().Next(10, 15);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                //Guardar los datos de la página en la variable bibtex_data
                string bibtexData = driver.PageSource;

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(bibtexData);
                var preElement = htmlDocument.DocumentNode.SelectSingleNode("//pre");

                if (preElement != null)
                {
                    bibtexData = preElement.InnerText;
                    Console.WriteLine("Datos BibTeX:");
                    Console.WriteLine(bibtexData);

                    // Extraer datos de BibTeX
                    string titulo = ExtractBibTeXValue(bibtexData, "title");
                    string autor = ExtractBibTeXValue(bibtexData, "author");
                    string revista = ExtractBibTeXValue(bibtexData, "journal");
                    string año = ExtractBibTeXValue(bibtexData, "year");
                    string editor = ExtractBibTeXValue(bibtexData, "publisher");
                    
                    DataRow row = dataTable.NewRow();
                    row["Titulo"] = titulo;
                    row["Autor"] = autor;
                    row["Revista"] = revista;
                    row["Año"] = año;
                    row["Editor"] = editor;
                    row["URL"] = link;
                    row["Citado"] = numCitas;

                    dataTable.Rows.Add(row);
                }
                else
                {
                    Console.WriteLine("No se encontraron datos BibTeX válidos para el artículo: " + title);
                }
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("No se encontró el enlace 'BibTeX' para el artículo: " + title);
            }
            // Volver atras
            driver.Navigate().Back();
            secondsPause = new Random().Next(15, 20);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
            
            //volver atras
            driver.Navigate().Back();
            secondsPause = new Random().Next(15, 20);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
        }
        

        // Almacena el DataTable en un archivo CSV
        using (var writer = new StreamWriter("web_scraping_citas.csv"))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(dataTable.AsEnumerable());
        }

        System.Threading.Thread.Sleep(60000); // Espera 60 segundos
        driver.Quit();
        Console.WriteLine("Datos obtenidos: ");
        foreach (DataRow row in dataTable.Rows)
        {
            Console.WriteLine(row["Titulo"]);
            Console.WriteLine(row["Autor"]);
            Console.WriteLine(row["Revista"]);
            Console.WriteLine(row["Año"]);
            Console.WriteLine(row["Editor"]);
            Console.WriteLine(row["URL"]);
            Console.WriteLine(row["Citado"]);
        }
    }

    static int ExtractCitationCount(string citadoPor)
    {
        var match = System.Text.RegularExpressions.Regex.Match(citadoPor, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }
    static string ExtractBibTeXValue(string bibtexData, string key)
    {
        var regex = new System.Text.RegularExpressions.Regex(key + @"\s*=\s*{([^}]*)}");
        var match = regex.Match(bibtexData);
        return match.Success ? match.Groups[1].Value : "";
    }
}
