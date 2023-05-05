const express = require('express');
const multer = require('multer');
const { execFile } = require('child_process');
const fs = require('fs');
const path = require('path');

const app = express();
const storage = multer.diskStorage({
    destination: (req, file, cb) => {
      cb(null, 'uploads/');
    },
    filename: (req, file, cb) => {
      cb(null, file.originalname);
    },
  });
  
  const upload = multer({ storage: storage });
const port = process.env.PORT || 80;
app.use(express.json()); 

app.post('/convert', upload.single('file'), (req, res) => {
  const { convertTo } = req.body;
  const inputFile = req.file.path;
  console.log(inputFile)
  const fileWithoutExtension = req.file.filename.substring(0,req.file.filename.lastIndexOf('.'));
  const outputFile = path.join(req.file.destination, `${fileWithoutExtension}.${convertTo}`);

  execFile('unoconvert', ['--convert-to', convertTo, inputFile, outputFile], (error) => {
    if (error) {
      console.error(error);
      fs.unlink(inputFile, () => {});
      res.status(500).send('Conversion failed');
    } else {
      res.download(outputFile, () => {
        fs.unlink(inputFile, () => {});
        fs.unlink(outputFile, () => {});
      });
    }
  });
});

app.post('/compare', upload.fields([{ name: 'newFile'}, { name: 'originalFile' }]), (req, res) => {
  const newFile = req.files.newFile[0].path;
  const originalFile = req.files.originalFile[0].path;
  const outputFile = path.join(req.files.newFile[0].destination, `result_compare.pdf`);
  console.log(newFile,originalFile)
  // execFile('libreoffice', [
  //   '--headless',
  //   '--invisible',
  //   '--nocrashreport',
  //   '--nodefault',
  //   '--nologo',
  //   '--nofirststartwizard',
  //   '--norestore',
  //   '--infilter="writer_pdf_import"',
  //   '--convert-to',
  //   'docx', 
  //    `"/app/${newFile}"`], (error) => {
  //   if (error) {console.log(error);}
  // });
  execFile('unocompare', [ newFile, originalFile, outputFile], (error) => {
    if (error) {
      console.error(error);
        fs.unlink(newFile, () => {});
        fs.unlink(originalFile, () => {});
      res.status(500).send('Comparison failed');
    } else {
      res.download(outputFile, () => {
        fs.unlink(newFile, () => {});
        fs.unlink(originalFile, () => {});
        fs.unlink(outputFile, () => {});
      });
    }
  });
});

// Start the server
app.listen(port, () => {
    console.log(`Proxy server listening at http://localhost:${port}`);
  });
