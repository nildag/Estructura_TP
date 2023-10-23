const puppeteer = require('puppeteer');
const fs = require('fs');
const { Parser } = require('json2csv');

(async () => {
  const browser = await puppeteer.launch({ headless: false });
  const page = await browser.newPage();

  await page.goto('https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es');
  
  await page.waitForSelector('#gs_res_ccl', { timeout: 10000 });

  const articulos = await page.$x('//*[@id="gs_res_ccl"]//h3');

  const data = [];

  for (let i = 0; i < articulos.length; i++) {
    const title = await (await articulos[i].getProperty('textContent')).jsonValue();
    const div_index = i + 1;

    try {
      const linkElement = await page.waitForXPath(`//*[@id="gs_res_ccl_mid"]/div[${div_index}]/div[2]/div[3]/a[3]`);
      const link = await linkElement.getProperty('href');
      console.log(await link.jsonValue());
    } catch (e) {
      console.error("Error al encontrar el elemento:", e);
    }

    const citadoPorElement = await articulos[i].$x(`//*[@id="gs_res_ccl_mid"]/div[${div_index}]/div[2]/div[3]/a[3]`);
    const citadoPor = citadoPorElement.length > 0 ? await (await citadoPorElement[0].getProperty('textContent')).jsonValue() : "No disponible";

    const match = citadoPor.match(/\d+/);
    const num_citas = match ? parseInt(match[0]) : 0;

    console.log("Citado por:", num_citas);

    try {
        const enlaceCitar = await page.waitForXPath(`//*[@id="gs_res_ccl_mid"]/div[${div_index}]/div[2]/div[3]/a[2]`);
        console.log("Enlace Citar:", await enlaceCitar.getProperty('href').jsonValue());
        const secondsPause = randomInt(10, 15);
        await sleep(secondsPause);
        enlaceCitar.click();
        
      try {
        const enlaceBibTeX = await page.waitForXPath('//*[@id="gs_citi"]/a[1]');
        console.log("Enlace BibTeX:", await enlaceBibTeX.getProperty('href').jsonValue());
        await enlaceBibTeX.click();
        const secondsPause = randomInt(10, 15);
        await sleep(secondsPause);
        
        const bibtex_data = await page.evaluate(() => document.querySelector('#gs_citi pre').textContent);
        console.log("Datos BibTeX:");
        console.log(bibtex_data);

        try {
          const parsed_entry = JSON.parse(bibtex_data);
          const new_data = [{
            'Titulo': parsed_entry.title,
            'Autor': parsed_entry.author,
            'Revista': parsed_entry.journal,
            'Año': parsed_entry.year,
            'Editor': parsed_entry.publisher,
            'URL': link,
            'Citado': num_citas,
          }];

          data.push(new_data);
        } catch (e) {
          console.error("Error al procesar los datos BibTeX:", e);
          data.push(null);
        }

        await page.goBack();
        secondsPause = randomInt(15, 20);
        await sleep(secondsPause);
      } catch (e) {
        console.error("No se encontró el enlace 'BibTeX' para el artículo:", e);
        data.push(null);
      }

      await page.goBack();
      secondsPause = randomInt(20, 35);
      await sleep(secondsPause);
    } catch (e) {
      console.error("No se encontró el enlace 'Citar' para el artículo:", e);
    }
  }

  // Almacenar los datos en un archivo CSV
  const fields = ['Titulo', 'Autor', 'Revista', 'Año', 'Editor', 'URL', 'Citado'];
  const json2csv = new Parser({ fields });
  const csv = json2csv.parse(data);
  fs.writeFileSync('web_scraping_citas.csv', csv, 'utf-8');
  await sleep(60000);

  await browser.close();

  console.log("Datos obtenidos: ");
  console.log(data);
})();

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function randomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}
