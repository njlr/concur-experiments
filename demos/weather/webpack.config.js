const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

const isProduction = process.env['NODE_ENV'] === 'production';

const mode = isProduction ? 'production' : 'development';

console.log({ mode });

module.exports = {
  mode,
  devtool: isProduction ? false : 'eval-source-map',
  entry: './weather.fsproj',
  output: {
    path: path.join(__dirname, './out'),
    filename: 'bundle.js',
  },
  devServer: {
    contentBase: path.join(__dirname, './public'),
    port: 8080,
    proxy: {
      '/api': {
        target: 'https://www.metaweather.com',
        secure: true,
        changeOrigin: true,
      },
    },
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: 'fable-loader',
      },
    ],
  },
  plugins: [
    new HtmlWebpackPlugin({
      templateContent: `
        <html>
          <body>
            <div id="root"></div>
          </body>
        </html>
      `,
      inject: true,
    }),
  ],
};
