namespace SacnOverIp;
        // class declarations
         class SacnSlave;
         class CommonE1_31;
         class RootLayer;
         class FramingLayer;
         class DmpLayer;
         class SacnMaster;
         class Connection;
     class SacnSlave 
    {
        // class delegates
        delegate FUNCTION DelegateFn ( );

        // class events

        // class functions
        FUNCTION InitializeFields ( INTEGER universe );
        SIGNED_LONG_INTEGER_FUNCTION GetTime ();
        STRING_FUNCTION ToString ();
        SIGNED_LONG_INTEGER_FUNCTION GetHashCode ();

        // class variables
        INTEGER __class_id__;

        // class properties
        DelegateProperty DelegateFn ReadSacn;
        INTEGER DmxData[];
    };

     class CommonE1_31 
    {
        // class delegates

        // class events

        // class functions
        STRING_FUNCTION ToString ();
        SIGNED_LONG_INTEGER_FUNCTION GetHashCode ();

        // class variables
        INTEGER __class_id__;

        // class properties
    };

     class RootLayer 
    {
        // class delegates

        // class events

        // class functions
        STRING_FUNCTION ToString ();
        SIGNED_LONG_INTEGER_FUNCTION GetHashCode ();

        // class variables
        INTEGER __class_id__;

        // class properties
    };

     class DmpLayer 
    {
        // class delegates

        // class events

        // class functions
        STRING_FUNCTION ToString ();
        SIGNED_LONG_INTEGER_FUNCTION GetHashCode ();

        // class variables
        INTEGER __class_id__;

        // class properties
    };

     class SacnMaster 
    {
        // class delegates

        // class events

        // class functions
        FUNCTION InitializeConnection ( STRING ipAddress );
        FUNCTION InitializeFields ( STRING sourceName , INTEGER universe );
        FUNCTION Send ( STRING activeDataSlotsAsString );
        STRING_FUNCTION ToString ();
        SIGNED_LONG_INTEGER_FUNCTION GetHashCode ();

        // class variables
        INTEGER __class_id__;

        // class properties
    };

