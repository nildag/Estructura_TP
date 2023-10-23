using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using CsvHelper;
using CsvHelper.Configuration.Attributes;
using WordCloudSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;



class Program2
{
    static void Main(string[] args)
    {
        // Cargar el DataFrame desde el archivo CSV
        var records = LoadCSV<Articulo>("web_scraping_citas.csv");

        // Visualizar las primeras filas del DataFrame
        Console.WriteLine("Primeras 5 filas del DataFrame:");
        foreach (var record in records.Take(5))
        {
            Console.WriteLine($"{record.Citado}, {record.Año}, {record.Titulo}, {record.Revista}");
        }

        // 1 - Ordenar el DataFrame por número de citas en orden descendente
        var df_ordenado = records.OrderByDescending(record => record.Citado).ToList();
        Console.WriteLine("Artículos ordenados por número de citas en orden descendente:");
        foreach (var record in df_ordenado)
        {
            Console.WriteLine($"{record.Citado}, {record.Año}, {record.Titulo}, {record.Revista}");
        }

        // Seleccionar los 10 artículos con más citas
        var top_10_citados = df_ordenado.Take(10).ToList();
        Console.WriteLine("Los 10 artículos más citados:");
        foreach (var record in top_10_citados)
        {
            Console.WriteLine($"{record.Citado}, {record.Año}, {record.Titulo}, {record.Revista}");
        }

        // Crear un nuevo DataFrame con las columnas deseadas
        var resultado = top_10_citados.Select(record => new { record.Citado, record.Año, record.Titulo, record.Revista }).ToList();
        Console.WriteLine("Nuevo DataFrame con las columnas deseadas:");
        foreach (var record in resultado)
        {
            Console.WriteLine($"{record.Citado}, {record.Año}, {record.Titulo}, {record.Revista}");
        }

        // Guardar el resultado en un archivo CSV
        SaveCSV(resultado, "top_10_articulos_citados.csv");

        // 2 - Solicitar al usuario que ingrese la palabra clave de interés
        Console.Write("Ingrese la palabra clave de interés: ");
        string palabra_clave = Console.ReadLine();

        // Filtrar los artículos que contienen la palabra clave en el título
        var resultados = records.Where(record => record.Titulo.Contains(palabra_clave, StringComparison.OrdinalIgnoreCase)).ToList();
        Console.WriteLine($"Artículos que contienen '{palabra_clave}' en el título:");
        foreach (var record in resultados)
        {
            Console.WriteLine($"{record.Titulo}, {record.URL}");
        }

        // Crear un CSV con los resultados
        SaveCSV(resultados.Select(record => new { record.Titulo, record.URL }), "resultados_palabra_clave.csv");
        Console.WriteLine("Se han guardado los resultados en 'resultados_palabra_clave.csv'.");

        // 3 - Extraer la lista de autores
        var autores = records.Select(record => record.Autor.Split(" and ")).ToList();

        // Crear una lista plana de autores
        var lista_autores = autores.Where(autor => autor != null).SelectMany(autores_articulo => autores_articulo).ToList();

        // Contar la frecuencia de cada autor
        var frecuencia_autores = lista_autores.GroupBy(autor => autor)
            .Select(group => new { Autor = group.Key, Frecuencia = group.Count() })
            .OrderByDescending(record => record.Frecuencia)
            .ToList();

        Console.WriteLine("Frecuencia de autores:");
        foreach (var record in frecuencia_autores)
        {
            Console.WriteLine($"{record.Autor}, {record.Frecuencia}");
        }

        // Guardar el resultado en un archivo CSV
        SaveCSV(frecuencia_autores, "autores_frecuencia.csv");
        Console.WriteLine("Se ha guardado la frecuencia de los autores en 'autores_frecuencia.csv'.");

        // 4 - Procesar los títulos y crear una nube de palabras
        var titulos = records.Select(record => record.Titulo).ToList();

        // Crear una lista de palabras significativas para omitir
        var palabrasOmitir = new string[] { "a", "an", "the", "in", "of", "for", "and", "on", "with", "to", "by", "at" };

        // Función para procesar los títulos y extraer palabras significativas
        IEnumerable<string> ProcesarTitulo(string titulo)
        {
            var palabras = Regex.Matches(titulo.ToLower(), @"\w+");
            foreach (Match match in palabras)
            {
                var palabra = match.Value;
                if (!palabrasOmitir.Contains(palabra))
                {
                    yield return palabra;
                }
            }
        }

        // Procesar los títulos y contar las palabras
        var todasPalabras = titulos.SelectMany(ProcesarTitulo).ToList();
        var contadorPalabras = todasPalabras.GroupBy(palabra => palabra)
            .Select(group => new { Palabra = group.Key, Frecuencia = group.Count() })
            .ToList();

        // Crear una nube de palabras
        var wordcloud = new WordCloud(800, 400);

        List<string> words = new List<string>();
        List<int> freqs = new List<int>();
        foreach (var palabra in contadorPalabras)
        {
            words.Add(palabra.Palabra);
            freqs.Add(palabra.Frecuencia);
        }

        // Crear una imagen de fondo con un color blanco
        var backgroundColor = Color.White;
        var backgroundImg = new Bitmap(800, 400);
        using (var g = Graphics.FromImage(backgroundImg))
        {
            g.Clear(backgroundColor);
        }
        
        // Generar la imagen de la nube de palabras
        var wordCloudImage = wordcloud.Draw(words,freqs,backgroundImg);
        //Guardar la imagen
        wordCloudImage.Save("wordcloud.png");
        Console.WriteLine("Nube de palabras generada y guardada en 'wordcloud.png'.");

        // Contar la cantidad de artículos recuperados por año
        var cAnios = records.GroupBy(record => record.Año)
            .Select(group => new { Anio = group.Key, Cantidad = group.Count() })
            .OrderBy(record => record.Anio)
            .ToList();

        List<AnioCantidad> conteoAnios = cAnios.Select(item => new AnioCantidad { Anio = item.Anio, Cantidad = item.Cantidad }).ToList();

        // Crear un gráfico de barras como una imagen
        Bitmap chartImage = CreateBarChart(conteoAnios);

        // Guardar la imagen del gráfico de barras como un archivo PNG
        chartImage.Save("barchart.png", ImageFormat.Png);

        Console.WriteLine("Gráfico de barras generado y guardado como 'barchart.png'.");
        
        Console.WriteLine("Nube de palabras y gráfico de barras generados.");
        
    }
    
    static Bitmap CreateBarChart(List<AnioCantidad> data)
    {
        int chartWidth = 800;
        int chartHeight = 400;

        Bitmap chartImage = new Bitmap(chartWidth, chartHeight);
        using (Graphics g = Graphics.FromImage(chartImage))
        {
            g.Clear(Color.White);

            // Definir los valores máximos y mínimos de datos
            int maxCantidad = data.Max(d => d.Cantidad);
            int minAnio = data.Min(d => d.Anio);
            int maxAnio = data.Max(d => d.Anio);

            // Definir márgenes y tamaños
            int margin = 20;
            int barWidth = (chartWidth - 2 * margin) / data.Count;
            int maxValueHeight = chartHeight - 2 * margin;

            // Dibujar barras
            using (Brush barBrush = new SolidBrush(Color.Blue))
            using (Pen barOutlinePen = new Pen(Color.Black))
            using (Font axisFont = new Font("Arial", 10))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    AnioCantidad item = data[i];
                    int x = margin + i * barWidth;
                    int barHeight = (int)(((double)item.Cantidad / maxCantidad) * maxValueHeight);
                    int y = chartHeight - margin - barHeight;

                    g.FillRectangle(barBrush, x, y, barWidth, barHeight);
                    g.DrawRectangle(barOutlinePen, x, y, barWidth, barHeight);

                    string labelText = item.Anio.ToString();
                    SizeF labelSize = g.MeasureString(labelText, axisFont);
                    g.DrawString(labelText, axisFont, Brushes.Black, x + (barWidth - labelSize.Width) / 2, chartHeight - margin + 5);
                }
            }
        }

        return chartImage;
    }

    static List<T> LoadCSV<T>(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            return csv.GetRecords<T>().ToList();
        }
    }

    static void SaveCSV<T>(IEnumerable<T> records, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(records);
        }
    }
}

[Serializable]
class Articulo
{
    [Name("Citado")]
    public int Citado { get; set; }

    [Name("Año")]
    public int Año { get; set; }

    [Name("Titulo")]
    public string Titulo { get; set; }

    [Name("Revista")]
    public string Revista { get; set; }

    [Name("URL")]
    public string URL { get; set; }

    [Name("Autor")]
    public string Autor { get; set; }
}
class AnioCantidad
{
    public int Anio { get; set; }
    public int Cantidad { get; set; }
}
