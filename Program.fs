open System

open WasmTypes
open WasmInstruction
open WatFunction
open WatData
open WatModule
open WatWriter

let makeTestModule () =
    {
        Imports = [
            {
                Source = "wasi_unstable"
                Name = "fd_write"
                ParameterTypes = [I32; I32; I32; I32]
                ResultTypes = [I32]
            }
        ]
        Functions = [
            {
                Name = "$console_writeline"
                Parameters = [
                    ("$ptr", I32);
                    ("$len", I32)
                ]
                ResultTypes = []
                Instructions = [
                    // Set up the ptr-len struct at mem addr 0
                    I32Const(0)
                    LocalGet(LocalId("$ptr"))
                    I32Store
                    I32Const(4)
                    LocalGet(LocalId("$len"))
                    I32Store
                    I32Const(1)  // fd - 1=stdout
                    I32Const(0)  // ptr to ptr-len struct(s)
                    I32Const(1)  // count of ptr-len struct(s)
                    I32Const(20)  // where to store nwritten
                    Call(FuncId("$fd_write"))
                    Drop
                ]
                Export = None
            }
            {
                Name = "$main"
                Parameters = []
                ResultTypes = []
                Instructions = [
                    I32Const(8)
                    I32Const(14)
                    Call(FuncId("$console_writeline"))
                ]
                Export = Some("_start")
            }
        ]
        Data = [
            {
                Offset = 8
                Content = StringContent("kia ora world\\n")
            }
        ]
    }

[<EntryPoint>]
let main argv =
    let testModule = makeTestModule()
    let writer = WatWriter.New()
    writeModule writer testModule
    printfn "%s" (text writer)
    System.IO.File.WriteAllText("testdata/gen.wat", (text writer))
    0
