import random
from bibtexparser import loads
from time import sleep
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import pandas as pd

# Crear un DataFrame con los datos
df = pd.DataFrame({
    'Título': [],  # Crea columnas vacías para los campos
    'Autor': [],
    'Revista': [],
    'Volumen': [], #no hace falta
    'Número': [], #no hace falta
    'Páginas': [], #no hace falta
    'Año': [],
    'Editor': []
})

driver = webdriver.Chrome()
driver.get('https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es')
# Esperar hasta que los resultados de la búsqueda se carguen (ajusta el tiempo máximo según tu necesidad)
wait = WebDriverWait(driver, 10)
wait.until(EC.presence_of_element_located((By.ID, 'gs_res_ccl')))

#Todos los articulos de una pagina
articulos = driver.find_elements(By.XPATH, '//*[@id="gs_res_ccl"]//h3')
secondsPause = 1
data = []

for articulo in articulos:
    titleArticle = articulo.text
    print("Título:", titleArticle)

    # Interactuar con "Citar"
    try:
        enlaceCitar = articulo.find_element(By.XPATH, '//*[@id="gs_res_ccl_mid"]/div[1]/div[2]/div[3]/a[2]')
        print("Enlace Citar:", enlaceCitar.get_attribute("href"))
        enlaceCitar.click()
        secondsPause = random.randrange(10, 15)
        sleep(secondsPause)

        # Intentar interactuar con "BibTeX"
        try:
            enlaceBibTeX = driver.find_element(By.XPATH, '//*[@id="gs_citi"]/a[1]')
            print("Enlace BibTeX:", enlaceBibTeX.get_attribute("href"))
            enlaceBibTeX.click()
            secondsPause = random.randrange(10, 15)
            sleep(secondsPause)
            # Guardar los datos de la página en el array
            bibtex_entry = driver.page_source
            data.append(bibtex_entry)

            # Analizar el BibTeX y acceder a los campos
            parsed_entry = loads(bibtex_entry)
            if parsed_entry.entries:
                entry = parsed_entry.entries[0]
                title = entry.get('title')
                author = entry.get('author')
                journal = entry.get('journal')
                volume = entry.get('volume')
                number = entry.get('number')
                pages = entry.get('pages')
                year = entry.get('year')
                publisher = entry.get('publisher')

                print("Title:", title)
                print("Author:", author)
                # ... y así sucesivamente para otros campos
            # Volver atras
            driver.back()
        except Exception as e:
            print("No se encontró el enlace 'BibTeX' para el artículo:", titleArticle)
            data.append(None)  # Almacenar None cuando hay una excepción

        #Volver atras
        driver.back()
        secondsPause = random.randrange(10, 15)
        sleep(secondsPause)


    except Exception as e:
        print("No se encontró el enlace 'Citar' para el artículo:", titleArticle)
        print(e)

for bibtex_entry in data:
    if bibtex_entry is not None:
        parsed_entry = loads(bibtex_entry)
        if parsed_entry.entries:
            entry = parsed_entry.entries[0]
            # Añadir datos a las columnas
            df = df.append({
                'Título': entry.get('title'),
                'Autor': entry.get('author'),
                'Revista': entry.get('journal'),
                'Volumen': entry.get('volume'),
                'Número': entry.get('number'),
                'Páginas': entry.get('pages'),
                'Año': entry.get('year'),
                'Editor': entry.get('publisher')
            }, ignore_index=True)

# Almacenar el DataFrame en un archivo CSV
df.to_csv('web_scraping_citas.csv', index=False)
sleep(60)
driver.quit()
print("Datos obtenidos: ",df.head())