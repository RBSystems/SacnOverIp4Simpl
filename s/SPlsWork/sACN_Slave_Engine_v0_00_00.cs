using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using Crestron;
using Crestron.Logos.SplusLibrary;
using Crestron.Logos.SplusObjects;
using Crestron.SimplSharp;
using SacnOverIp;

namespace UserModule_SACN_SLAVE_ENGINE_V0_00_00
{
    public class UserModuleClass_SACN_SLAVE_ENGINE_V0_00_00 : SplusObject
    {
        static CCriticalSection g_criticalSection = new CCriticalSection();
        
        
        UShortParameter UNIVERSE;
        InOutArray<Crestron.Logos.SplusObjects.StringOutput> SLOTS;
        SacnOverIp.SacnSlave SLAVE;
        private void REGISTERDELEGATES (  SplusExecutionContext __context__ ) 
            { 
            
            __context__.SourceCodeLine = 73;
            // RegisterDelegate( SLAVE , READSACN , READJOINS ) 
            SLAVE .ReadSacn  = READJOINS; ; 
            
            }
            
        private void INITIALIZESACN (  SplusExecutionContext __context__ ) 
            { 
            
            __context__.SourceCodeLine = 78;
            SLAVE . InitializeFields ( (ushort)( UNIVERSE  .Value )) ; 
            
            }
            
        public void READJOINS ( ) 
            { 
            CrestronString DMX64;
            DMX64  = new CrestronString( Crestron.Logos.SplusObjects.CrestronStringEncoding.eEncodingASCII, 192, this );
            
            ushort Y = 0;
            
            ushort X = 0;
            
            ushort I = 0;
            
            int TIMES = 0;
            
            ushort CHUNKS = 0;
            
            ushort Z = 0;
            
            try
            {
                SplusExecutionContext __context__ = SplusSimplSharpDelegateThreadStartCode();
                
                __context__.SourceCodeLine = 91;
                CHUNKS = (ushort) ( 4 ) ; 
                __context__.SourceCodeLine = 92;
                Z = (ushort) ( (CHUNKS - 1) ) ; 
                __context__.SourceCodeLine = 94;
                ushort __FN_FORSTART_VAL__1 = (ushort) ( 0 ) ;
                ushort __FN_FOREND_VAL__1 = (ushort)Z; 
                int __FN_FORSTEP_VAL__1 = (int)1; 
                for ( X  = __FN_FORSTART_VAL__1; (__FN_FORSTEP_VAL__1 > 0)  ? ( (X  >= __FN_FORSTART_VAL__1) && (X  <= __FN_FOREND_VAL__1) ) : ( (X  <= __FN_FORSTART_VAL__1) && (X  >= __FN_FOREND_VAL__1) ) ; X  += (ushort)__FN_FORSTEP_VAL__1) 
                    { 
                    __context__.SourceCodeLine = 96;
                    TIMES = (int) ( SLAVE.GetTime() ) ; 
                    __context__.SourceCodeLine = 97;
                    Print( "S+ start Block {0:d} of {1:d}: {2:d}\r\n", (short)(X + 1), (short)CHUNKS, (int)TIMES) ; 
                    __context__.SourceCodeLine = 99;
                    Y = (ushort) ( ((X * 512) / CHUNKS) ) ; 
                    __context__.SourceCodeLine = 100;
                    DMX64  =  ( ""  )  .ToString() ; 
                    __context__.SourceCodeLine = 101;
                    ushort __FN_FORSTART_VAL__2 = (ushort) ( Y ) ;
                    ushort __FN_FOREND_VAL__2 = (ushort)(Y + ((512 / CHUNKS) - 1)); 
                    int __FN_FORSTEP_VAL__2 = (int)1; 
                    for ( I  = __FN_FORSTART_VAL__2; (__FN_FORSTEP_VAL__2 > 0)  ? ( (I  >= __FN_FORSTART_VAL__2) && (I  <= __FN_FOREND_VAL__2) ) : ( (I  <= __FN_FORSTART_VAL__2) && (I  >= __FN_FOREND_VAL__2) ) ; I  += (ushort)__FN_FORSTEP_VAL__2) 
                        { 
                        __context__.SourceCodeLine = 105;
                        MakeString ( DMX64 , "{0}{1:x2}", DMX64 , SLAVE.DmxData[ I ]) ; 
                        __context__.SourceCodeLine = 101;
                        } 
                    
                    __context__.SourceCodeLine = 107;
                    SLOTS [ (X + 1)]  .UpdateValue ( DMX64  ) ; 
                    __context__.SourceCodeLine = 94;
                    } 
                
                __context__.SourceCodeLine = 110;
                TIMES = (int) ( SLAVE.GetTime() ) ; 
                __context__.SourceCodeLine = 111;
                Print( "S+ end: {0:d}\r\n", (int)TIMES) ; 
                
                
            }
            finally { ObjectFinallyHandler(); }
            }
            
        public override object FunctionMain (  object __obj__ ) 
            { 
            try
            {
                SplusExecutionContext __context__ = SplusFunctionMainStartCode();
                
                __context__.SourceCodeLine = 150;
                WaitForInitializationComplete ( ) ; 
                __context__.SourceCodeLine = 151;
                REGISTERDELEGATES (  __context__  ) ; 
                __context__.SourceCodeLine = 152;
                INITIALIZESACN (  __context__  ) ; 
                
                
            }
            catch(Exception e) { ObjectCatchHandler(e); }
            finally { ObjectFinallyHandler(); }
            return __obj__;
            }
            
        
        public override void LogosSplusInitialize()
        {
            SocketInfo __socketinfo__ = new SocketInfo( 1, this );
            InitialParametersClass.ResolveHostName = __socketinfo__.ResolveHostName;
            _SplusNVRAM = new SplusNVRAM( this );
            
            SLOTS = new InOutArray<StringOutput>( 8, this );
            for( uint i = 0; i < 8; i++ )
            {
                SLOTS[i+1] = new Crestron.Logos.SplusObjects.StringOutput( SLOTS__AnalogSerialOutput__ + i, this );
                m_StringOutputList.Add( SLOTS__AnalogSerialOutput__ + i, SLOTS[i+1] );
            }
            
            UNIVERSE = new UShortParameter( UNIVERSE__Parameter__, this );
            m_ParameterList.Add( UNIVERSE__Parameter__, UNIVERSE );
            
            
            
            _SplusNVRAM.PopulateCustomAttributeList( true );
            
            NVRAM = _SplusNVRAM;
            
        }
        
        public override void LogosSimplSharpInitialize()
        {
            SLAVE  = new SacnOverIp.SacnSlave();
            
            
        }
        
        public UserModuleClass_SACN_SLAVE_ENGINE_V0_00_00 ( string InstanceName, string ReferenceID, Crestron.Logos.SplusObjects.CrestronStringEncoding nEncodingType ) : base( InstanceName, ReferenceID, nEncodingType ) {}
        
        
        
        
        const uint UNIVERSE__Parameter__ = 10;
        const uint SLOTS__AnalogSerialOutput__ = 0;
        
        [SplusStructAttribute(-1, true, false)]
        public class SplusNVRAM : SplusStructureBase
        {
        
            public SplusNVRAM( SplusObject __caller__ ) : base( __caller__ ) {}
            
            
        }
        
        SplusNVRAM _SplusNVRAM = null;
        
        public class __CEvent__ : CEvent
        {
            public __CEvent__() {}
            public void Close() { base.Close(); }
            public int Reset() { return base.Reset() ? 1 : 0; }
            public int Set() { return base.Set() ? 1 : 0; }
            public int Wait( int timeOutInMs ) { return base.Wait( timeOutInMs ) ? 1 : 0; }
        }
        public class __CMutex__ : CMutex
        {
            public __CMutex__() {}
            public void Close() { base.Close(); }
            public void ReleaseMutex() { base.ReleaseMutex(); }
            public int WaitForMutex() { return base.WaitForMutex() ? 1 : 0; }
        }
         public int IsNull( object obj ){ return (obj == null) ? 1 : 0; }
    }
    
    
}
