library(data.table)
library(RSelenium)
library(rvest)


# Crear un data table con los datos
dataTable <- data.table(
  Titulo = character(0),
  Autor = character(0),
  Revista = character(0),
  Año = integer(0),
  Editor = character(0),
  URL = character(0),
  Citado = integer(0)
)

# Inicializa el navegador Selenium
driver <- rsDriver(browser = "chrome")
remote_driver <- driver[["client"]]

# Navega a la página
remote_driver$navigate("https://scholar.google.com")


# Esperar hasta que los resultados de la búsqueda se carguen
Sys.sleep(120)

# Extraer todos los artículos de una página
articulos <- remote_driver$findElements("css selector", "#gs_res_ccl h3")
data <- list()

for (i in 1:length(articulos)) {
  title <- articulos[[i]]$getElementText()
  divIndex <- i
  tryCatch({
    link <- remote_driver$findElement("css selector", paste0("#gs_res_ccl_mid div:nth-child(", divIndex, ") div:nth-child(2) div:nth-child(3) a:nth-child(3)"))$getElementAttribute("href")
    cat("Link:", link, "\n")
  }, error = function(e) {
    cat("Error al encontrar el elemento:", e$message, "\n")
    link <- NA
  })
  
  xpath <- paste0("#gs_res_ccl_mid div:nth-child(", divIndex, ") div:nth-child(2) div:nth-child(3) a:nth-child(3)")
  citadoPor <- tryCatch({
    citadoPor <- articulos[i]$findElement("css selector", xpath)$getElementText()
    citadoPor
  }, error = function(e) {
    "No disponible"
  })
  
  numCitas <- as.integer(gsub("\\D", "", citadoPor))
  if (is.na(numCitas)) numCitas <- 0
  
  cat("Citado por:", numCitas, "\n")
  
  tryCatch({
    enlaceCitar <- articulos[i]$findElement("css selector", paste0("#gs_res_ccl_mid div:nth-child(", divIndex, ") div:nth-child(2) div:nth-child(3) a:nth-child(2)"))
    cat("Enlace Citar:", enlaceCitar$getElementAttribute("href"), "\n")
    Sys.sleep(sample(10:15, 1))
    enlaceCitar$click()
    
    tryCatch({
      Sys.sleep(sample(10:15, 1))
      enlaceBibTeX <- remote_driver$findElement("css selector", "#gs_citi a:nth-child(1)")
      cat("Enlace BibTeX:", enlaceBibTeX$getElementAttribute("href"), "\n")
      enlaceBibTeX$click()
      Sys.sleep(sample(10:15, 1))
      
      # Guardar los datos de la página en la variable bibtex_data
      bibtex_data <- remote_driver$getPageSource()[[1]]
      
      # Utilizamos rvest para extraer los datos BibTeX
      bibtex_data <- bibtex_data %>%
        read_html() %>%
        html_element("pre") %>%
        html_text()
      cat("Datos BibTeX:\n")
      cat(bibtex_data, "\n")
      data[[i]] <- bibtex_data
      
      tryCatch({
        # Analizar el BibTeX y acceder a los campos
        parsed_entry <- parse.bib(text = bibtex_data)
        if (length(parsed_entry) > 0) {
          entry <- parsed_entry[[1]]
          # Crear una nueva fila en el data table
          newDataRow <- data.table(
            Titulo = entry$Title,
            Autor = entry$Author,
            Revista = entry$Journal,
            Año = entry$Year,
            Editor = entry$Publisher,
            URL = link,
            Citado = numCitas
          )
          dataTable <- rbind(dataTable, newDataRow)
        } else {
          # Manejar el caso en que no se encuentren datos BibTeX válidos
          cat("No se encontraron datos BibTeX válidos para el artículo:", title, "\n")
          data[[i]] <- NA
        }
      }, error = function(e) {
        cat("Error al procesar los datos BibTeX para el artículo:", title, "\n")
        data[[i]] <- NA
      })
      
      remote_driver$back()
      Sys.sleep(sample(15:20, 1))
    }, error = function(e) {
      cat("No se encontró el enlace 'BibTeX' para el artículo:", title, "\n")
      data[[i]] <- NA
    })
    
    remote_driver$back()
    Sys.sleep(sample(20:35, 1))
  }, error = function(e) {
    cat("No se encontró el enlace 'Citar' para el artículo:", title, "\n")
  })
}

# Almacenar el data table en un archivo CSV
fwrite(dataTable, "web_scraping_citas.csv")

# Cerrar el navegador Selenium
driver$server$stop()

# Mostrar los primeros registros del data table
cat("Datos obtenidos:", head(dataTable))
