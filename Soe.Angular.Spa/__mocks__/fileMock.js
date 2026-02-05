module.exports = {
    process(src, filename) {
        throw new Error('Transformer is being used');

        console.error(`Transforming file: ${filename}`);
        if (filename.endsWith('.html')) {
            return 'module.exports = ' + JSON.stringify(src);
        }
        if (filename.endsWith('.css')) {
            return 'module.exports = {};';
        }
        return src;
    },
};