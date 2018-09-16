module NumberTrackingService.Infrastructure.Logging

let private logFactory = NLog.LogManager.LoadConfiguration("nlog.config")

// TODO: Determine how to provide exceptions to log messages
type LogMessage = 
    Trace of string
    | Debug of string
    | Info of string
    | Warn of string
    | Error of string
    | Fatal of string    

type Logger = LogMessage -> unit
type LogFactory = string -> Logger

let logger name =    
    let l = logFactory.GetLogger name
    fun logMessage ->
        match logMessage with 
        | Trace msg -> l.Trace msg    
        | Debug msg -> l.Debug msg
        | Info msg -> l.Info msg
        | Warn msg -> l.Warn msg    
        | Error msg -> l.Error msg
        | Fatal msg -> l.Fatal msg
