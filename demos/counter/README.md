# counter

```bash
yarn install
yarn webpack-dev-server
```

## Note!

To fix the build error with `FSharp.Control.AsyncSeq`, you must open `.fable/FSharp.Control.AsyncSeq.3.0.1/FSharp.Control.AsyncSeq.fsproj` and remove this line:

```
    <Compile Include="AsyncSeq.fsi" />
```

See: https://github.com/fsprojects/FSharp.Control.AsyncSeq/issues/117
