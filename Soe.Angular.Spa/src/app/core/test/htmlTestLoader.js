const htmlLoader = require('html-loader');

module.exports = {
  process(src, filename) {
    // Mock the context object required by html-loader
    const context = {
      resourcePath: filename,
    };

    // Execute html-loader synchronously
    try {
      const processedCode = htmlLoader.call(context, src);
      console.log('HTML processed', processedCode);
      return {
        code: processedCode,
      };
    } catch (error) {
      throw new Error(`Error processing HTML: ${error.message}`);
    }
  },
};
