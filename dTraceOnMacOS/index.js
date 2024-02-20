const net = require('net');
const fs = require('fs');
const path = require('path');
const { app, BrowserWindow, ipcMain } = require('electron');
const url = require('url');

let win;
let earlyLogs = [];

ipcMain.on('navigate', (event, url) => {
  let newWin = new BrowserWindow({ width: 1000, height: 600 });
  newWin.loadURL(url);
});
// Replace console.log with a function that sends the log arguments to the renderer process
console.log = function() {
  const message = Array.from(arguments).join(' ');
  if (win && win.webContents) {
    win.webContents.send('log', message);
  } else {
    earlyLogs.push(message);
  }
};

function createWindow () {
  win = new BrowserWindow({
    width: 800,
    height: 600,
    title: 'dTrace GUI MacOS',
    icon: path.join(__dirname, 'dTrace.icns'),
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      devTools: false
    }
  });

  win.setMenuBarVisibility(false);

  win.loadURL(url.format({
    pathname: path.join(__dirname, 'index.html'),
    protocol: 'file:',
    slashes: true
  }));

  win.on('closed', () => {
    win = null;
  });

  earlyLogs.forEach(message => {
    win.webContents.send('log', message);
  });
  earlyLogs = [];
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (win === null) {
    createWindow();
  }
});



// Create a directory for log files
const logDirectory = path.join(process.env.HOME, 'Documents', 'dnelb_temp', 'dTrace');
fs.mkdirSync(logDirectory, { recursive: true });

// Generate a unique log file name based on the current timestamp
const date = new Date();
const day = date.getDate().toString().padStart(2, '0');
const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Months are 0-indexed
const year = date.getFullYear();
const hours = date.getHours().toString().padStart(2, '0');
const minutes = date.getMinutes().toString().padStart(2, '0');
const seconds = date.getSeconds().toString().padStart(2, '0');
const logFileName = path.join(logDirectory, `dTrace_Log_(${day}_${month}_${year})_[${hours}-${minutes}-${seconds}].txt`);


console.log('<a href="https://github.com/jsluganovic/Dnelb-dTrace">dTrace MacOS</a> [4] Dnelb by @jsluganovic v[2.0.0] > 20/02/2024');
// Open the log file

const logFileStream = fs.createWriteStream(logFileName, { flags: 'a' });

const server = net.createServer((socket) => {
    console.log('[INFO] Client connected.');

    socket.on('data', (data) => {
        const message = data.toString().trim();

        // Add a timestamp to the message
        const logMessage = `[${new Date().toISOString()}] ${message}`;
        logFileStream.write(logMessage + '\n');

        console.log(message);
    });

    socket.on('end', () => {
        console.log('[INFO] Client disconnected.');
    });
});

const socketPath = '/tmp/dTracePipe';

// Delete the socket file if it already exists
try {
    fs.unlinkSync(socketPath);
} catch (err) {
    // Ignore errors
}

server.on('error', (err) => {
    console.error(`[ERROR] Server error: ${err}`);
});

server.listen(socketPath, () => {
    console.log('[INFO] Waiting for connection...');
});

// Handle shutdown
function handleShutdown() {
    console.log('[INFO] Shutting down...');
    server.close(() => {
        console.log('[INFO] Server closed.');
    });
    logFileStream.end(() => {
        console.log('[INFO] Log file stream closed.');
    });
}

process.on('SIGINT', handleShutdown);
process.on('SIGTERM', handleShutdown);