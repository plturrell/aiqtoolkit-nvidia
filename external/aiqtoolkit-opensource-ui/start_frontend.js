const { spawn } = require('child_process');

// Start next dev
const next = spawn('npm', ['run', 'dev'], {
    stdio: 'inherit',
    env: { ...process.env }
});

// Handle exit
process.on('SIGINT', () => {
    next.kill('SIGINT');
    process.exit();
});

process.on('SIGTERM', () => {
    next.kill('SIGTERM');
    process.exit();
});
