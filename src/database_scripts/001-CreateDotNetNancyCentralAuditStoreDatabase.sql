� � U S E   [ m a s t e r ]  
 G O  
 / * * * * * *   O b j e c t :     D a t a b a s e   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]         S c r i p t   D a t e :   1 2 / 0 1 / 2 0 1 0   1 5 : 5 9 : 0 4   * * * * * * /  
 C R E A T E   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   O N     P R I M A R Y    
 (   N A M E   =   N ' D o t N e t N a n c y C e n t r a l A u d i t S t o r e ' ,   F I L E N A M E   =   N ' D : \ S Q L D a t a \ D o t N e t N a n c y C e n t r a l A u d i t S t o r e . m d f '   ,   S I Z E   =   3 0 7 2 K B   ,   M A X S I Z E   =   U N L I M I T E D ,   F I L E G R O W T H   =   1 0 2 4 K B   )  
   L O G   O N    
 (   N A M E   =   N ' D o t N e t N a n c y C e n t r a l A u d i t S t o r e _ l o g ' ,   F I L E N A M E   =   N ' E : \ S Q L L o g s \ D o t N e t N a n c y C e n t r a l A u d i t S t o r e _ l o g . l d f '   ,   S I Z E   =   3 0 7 2 K B   ,   M A X S I Z E   =   2 0 4 8 G B   ,   F I L E G R O W T H   =   1 0 % )  
 G O  
 E X E C   d b o . s p _ d b c m p t l e v e l   @ d b n a m e = N ' D o t N e t N a n c y C e n t r a l A u d i t S t o r e ' ,   @ n e w _ c m p t l e v e l = 9 0  
 G O  
 I F   ( 1   =   F U L L T E X T S E R V I C E P R O P E R T Y ( ' I s F u l l T e x t I n s t a l l e d ' ) ) 
 b e g i n 
 E X E C   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ] . [ d b o ] . [ s p _ f u l l t e x t _ d a t a b a s e ]   @ a c t i o n   =   ' d i s a b l e ' 
 e n d  
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A N S I _ N U L L _ D E F A U L T   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A N S I _ N U L L S   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A N S I _ P A D D I N G   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A N S I _ W A R N I N G S   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A R I T H A B O R T   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A U T O _ C L O S E   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A U T O _ C R E A T E _ S T A T I S T I C S   O N    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A U T O _ S H R I N K   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A U T O _ U P D A T E _ S T A T I S T I C S   O N    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   C U R S O R _ C L O S E _ O N _ C O M M I T   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   C U R S O R _ D E F A U L T     G L O B A L    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   C O N C A T _ N U L L _ Y I E L D S _ N U L L   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   N U M E R I C _ R O U N D A B O R T   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   Q U O T E D _ I D E N T I F I E R   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   R E C U R S I V E _ T R I G G E R S   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T     E N A B L E _ B R O K E R    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A U T O _ U P D A T E _ S T A T I S T I C S _ A S Y N C   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   D A T E _ C O R R E L A T I O N _ O P T I M I Z A T I O N   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   T R U S T W O R T H Y   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   A L L O W _ S N A P S H O T _ I S O L A T I O N   O F F    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   P A R A M E T E R I Z A T I O N   S I M P L E    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T     R E A D _ W R I T E    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   R E C O V E R Y   F U L L    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T     M U L T I _ U S E R    
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   P A G E _ V E R I F Y   C H E C K S U M      
 G O  
 A L T E R   D A T A B A S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]   S E T   D B _ C H A I N I N G   O F F   