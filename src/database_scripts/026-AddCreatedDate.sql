� � / *  
       W e d n e s d a y ,   M a r c h   2 3 ,   2 0 1 1 3 : 3 5 : 1 7   P M  
       U s e r :    
       S e r v e r :   l o c a l h o s t  
       D a t a b a s e :   D o t N e t N a n c y C e n t r a l A u d i t S t o r e  
       A p p l i c a t i o n :    
 * /  
  
 / *   T o   p r e v e n t   a n y   p o t e n t i a l   d a t a   l o s s   i s s u e s ,   y o u   s h o u l d   r e v i e w   t h i s   s c r i p t   i n   d e t a i l   b e f o r e   r u n n i n g   i t   o u t s i d e   t h e   c o n t e x t   o f   t h e   d a t a b a s e   d e s i g n e r . * /  
 B E G I N   T R A N S A C T I O N  
 S E T   Q U O T E D _ I D E N T I F I E R   O N  
 S E T   A R I T H A B O R T   O N  
 S E T   N U M E R I C _ R O U N D A B O R T   O F F  
 S E T   C O N C A T _ N U L L _ Y I E L D S _ N U L L   O N  
 S E T   A N S I _ N U L L S   O N  
 S E T   A N S I _ P A D D I N G   O N  
 S E T   A N S I _ W A R N I N G S   O N  
 C O M M I T  
 B E G I N   T R A N S A C T I O N  
 G O  
 A L T E R   T A B L E   d b o . A u d i t   A D D  
 	 C r e a t e d D a t e   d a t e t i m e   N O T   N U L L   C O N S T R A I N T   D F _ A u d i t _ C r e a t e d D a t e   D E F A U L T   G E T U T C D A T E ( )  
 G O  
 A L T E R   T A B L E   d b o . A u d i t   S E T   ( L O C K _ E S C A L A T I O N   =   T A B L E )  
 G O  
 C O M M I T  
 