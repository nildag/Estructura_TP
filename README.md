# Web Scraping y Análisis de Datos de Google Scholar

Repositorio con scripts en múltiples lenguajes (C#, Python, R, Ruby) para extraer y analizar citas académicas de Google Scholar.

## Estructura del Repositorio
/
├── CSharp/ # Proyectos C#

├── Python/ # Scripts Python

├── R/ # Código R

├── Ruby/ # Scripts Ruby

└── README.md # Este archivo


## Instrucciones por Lenguaje

### 🔷 C# 
**Ubicación:** `/CSharp`

#### Web Scraping
1. Requisitos:
   - Visual Studio Code o IDE compatible
2. Ejecutar desde terminal:
   cd CSharp/Scrape
   dotnet run
Resultados:
Genera web_scraping_citas.csv

Análisis de Datos
Ejecutar desde terminal:

cd CSharp/Procesamiento
dotnet run
Resultados:

CSV: top_10_articulos_citados.csv, resultados_palabra_clave.csv

Gráficos: wordcloud.png, bar_graph.png

🐍 Python
Ubicación: /Python

Web Scraping
Instalar dependencias:

pip install pandas selenium bibtexparser beautifulsoup4
Ejecutar:

cd Python
python pythonScrape.py
Resultados:
web_scraping_citas.csv

Análisis de Datos
Instalar dependencias:

pip install pandas matplotlib wordcloud
Ejecutar:

python pythonDataProc.py
Resultados:

Archivos CSV y visualizaciones interactivas

📊 R
Ubicación: /R

Web Scraping
Instalar paquetes:

install.packages(c("data.table", "RSelenium", "rvest"))
Ejecutar scrape.r en R/RStudio

Resultados:
web_scraping_citas.csv

Análisis de Datos
Instalar paquetes:

install.packages(c("tm", "wordcloud", "colorspace", "ggplot2"))
Editar ruta en dataproc.r (línea 11):

ruta_del_csv <- 'R/web_scraping_citas.csv'  # Ajustar según ubicación real
Ejecutar dataproc.r

Resultados:

CSV procesados

Gráficos: barras.png, nube_palabras.png

💎 Ruby
Ubicación: /Ruby

Web Scraping
Instalar gemas:


gem install selenium-webdriver csv nokogiri bibtex-ruby
Ejecutar:


cd Ruby
ruby scrape.rb
Resultados:
web_scraping_citas.csv

Análisis de Datos
Ejecutar:

ruby dataproc.rb
Resultados:
Archivos CSV procesados

Notas Generales
📂 Los archivos CSV generados se guardan en la carpeta de cada proyecto

⚠️ Para R y Ruby: Verificar rutas de archivos según tu estructura de directorios

🔒 Usar proxies si hay bloqueos de Google Scholar

🕒 Los tiempos de ejecución varían según la cantidad de datos
