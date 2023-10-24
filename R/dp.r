# Instala y carga las bibliotecas necesarias
install.packages("tm")
install.packages("wordcloud")
install.packages("colorspace")
install.packages("ggplot2")
library(tm)
library(wordcloud)
library(ggplot2)

# Cargar el DataFrame desde el archivo CSV
df <- read.csv('C:/Users/Moon/Desktop/Estructura_TP/R/web_scraping_citas.csv', stringsAsFactors = FALSE)

# Ordenar el DataFrame por número de citas en orden descendente
df_ordenado <- df[order(-df$Citado), ]

# Seleccionar los 10 artículos con más citas
top_10_citados <- head(df_ordenado, 10)

# Crear un nuevo DataFrame con las columnas deseadas
resultado <- top_10_citados[c('Citado', 'Año', 'Titulo', 'Revista')]

# Guardar el resultado en un archivo CSV
write.csv(resultado, 'top_10_articulos_citados.csv', row.names = FALSE)

# Solicitar al usuario que ingrese la palabra clave de interés
palabra_clave <- readline("Ingrese la palabra clave de interés: ")

# Filtrar los artículos que contienen la palabra clave en el título
resultados <- df[grep(palabra_clave, df$Titulo, ignore.case = TRUE), ]

# Crear un DataFrame con los títulos y URLs de los artículos que coinciden
resultados_filtrados <- resultados[c('Titulo', 'URL')]

# Guardar el resultado en un archivo CSV
write.csv(resultados_filtrados, 'resultados_palabra_clave.csv', row.names = FALSE)

# Extraer la lista de autores
autores <- strsplit(df$Autor, ' and ')

# Crear una lista plana de autores
lista_autores <- unlist(autores)

# Contar la frecuencia de cada autor
frecuencia_autores <- data.frame(table(lista_autores))
colnames(frecuencia_autores) <- c('Autor', 'Frecuencia')

# Ordenar la lista de autores por el número de veces que aparecen
frecuencia_autores <- frecuencia_autores[order(-frecuencia_autores$Frecuencia), ]

# Guardar el resultado en un archivo CSV
write.csv(frecuencia_autores, 'autores_frecuencia.csv', row.names = FALSE)

# Definir las palabras a omitir
palabras_omitir <- c('a', 'an', 'the', 'in', 'of', 'for', 'and', 'on', 'with', 'to', 'by', 'at')

# Procesar los títulos y contar las palabras
titulos <- df$Titulo
todas_palabras <- unlist(lapply(titulos, function(titulo) {
  palabras <- unlist(strsplit(tolower(titulo), "\\W+"))
  palabras_filtradas <- palabras[!(palabras %in% palabras_omitir)]
  return(palabras_filtradas)
}))

# Contar la frecuencia de palabras
contador_palabras <- table(todas_palabras)
print(contador_palabras)

# Crear un WordCloud
png("wordcloud.png", width = 800, height = 400)
wordcloud(words = names(contador_palabras), freq = contador_palabras, min.freq = 1, scale = c(3, 0.5), colors = brewer.pal(8, "Dark2"))
dev.off()

# Contar la cantidad de artículos recuperados por año
conteo_anios <- table(df$Año)

# Crear un gráfico de barras
barplot(conteo_anios, main = "Cantidad de Artículos por Año de Publicación", xlab = "Año", ylab = "Cantidad de Artículos")
