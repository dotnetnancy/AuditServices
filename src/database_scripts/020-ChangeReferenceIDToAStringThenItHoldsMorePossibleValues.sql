� � / *  
       W e d n e s d a y ,   M a r c h   2 3 ,   2 0 1 1 1 0 : 1 6 : 5 5   A M  
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
 A L T E R   T A B L E   d b o . A u d i t  
 	 D R O P   C O N S T R A I N T   F K _ A u d i t _ A p p l i c a t i o n  
 G O  
 A L T E R   T A B L E   d b o . A p p l i c a t i o n   S E T   ( L O C K _ E S C A L A T I O N   =   T A B L E )  
 G O  
 C O M M I T  
 B E G I N   T R A N S A C T I O N  
 G O  
 A L T E R   T A B L E   d b o . A u d i t  
 	 D R O P   C O N S T R A I N T   F K _ A u d i t _ S t a t u s  
 G O  
 A L T E R   T A B L E   d b o . S t a t u s   S E T   ( L O C K _ E S C A L A T I O N   =   T A B L E )  
 G O  
 C O M M I T  
 B E G I N   T R A N S A C T I O N  
 G O  
 A L T E R   T A B L E   d b o . A u d i t  
 	 D R O P   C O N S T R A I N T   F K _ A u d i t _ O r i g i n a t i o n  
 G O  
 A L T E R   T A B L E   d b o . O r i g i n a t i o n   S E T   ( L O C K _ E S C A L A T I O N   =   T A B L E )  
 G O  
 C O M M I T  
 B E G I N   T R A N S A C T I O N  
 G O  
 A L T E R   T A B L E   d b o . A u d i t  
 	 D R O P   C O N S T R A I N T   F K _ A u d i t _ A u d i t C a t e g o r y  
 G O  
 A L T E R   T A B L E   d b o . A u d i t C a t e g o r y   S E T   ( L O C K _ E S C A L A T I O N   =   T A B L E )  
 G O  
 C O M M I T  
 B E G I N   T R A N S A C T I O N  
 G O  
 A L T E R   T A B L E   d b o . A u d i t  
 	 D R O P   C O N S T R A I N T   D F _ A u d i t _ A u d i t I D  
 G O  
 A L T E R   T A B L E   d b o . A u d i t  
 	 D R O P   C O N S T R A I N T   D F _ A u d i t _ R e f e r e n c e I D  
 G O  
 C R E A T E   T A B L E   d b o . T m p _ A u d i t  
 	 (  
 	 A u d i t I D   u n i q u e i d e n t i f i e r   N O T   N U L L ,  
 	 A u d i t C a t e g o r y I D   u n i q u e i d e n t i f i e r   N O T   N U L L ,  
 	 A u d i t C a t e g o r y N a m e   n v a r c h a r ( 2 5 5 )   N O T   N U L L ,  
 	 O r i g i n a t i o n I D   u n i q u e i d e n t i f i e r   N O T   N U L L ,  
 	 R e c o r d L o c a t o r   n v a r c h a r ( 5 0 )   N O T   N U L L ,  
 	 S t a t u s I D   u n i q u e i d e n t i f i e r   N O T   N U L L ,  
 	 D a t e T i m e S t a m p   d a t e t i m e   N O T   N U L L ,  
 	 M e s s a g e   n v a r c h a r ( 1 0 2 4 )   N O T   N U L L ,  
 	 Q N u m b e r   i n t   N U L L ,  
 	 Q C a t e g o r y   i n t   N U L L ,  
 	 P C C   n v a r c h a r ( 5 0 )   N U L L ,  
 	 A p p l i c a t i o n I D   u n i q u e i d e n t i f i e r   N O T   N U L L ,  
 	 R e f e r e n c e I D   n v a r c h a r ( 2 5 5 )   N U L L  
 	 )     O N   [ P R I M A R Y ]  
 G O  
 A L T E R   T A B L E   d b o . T m p _ A u d i t   S E T   ( L O C K _ E S C A L A T I O N   =   T A B L E )  
 G O  
 A L T E R   T A B L E   d b o . T m p _ A u d i t   A D D   C O N S T R A I N T  
 	 D F _ A u d i t _ A u d i t I D   D E F A U L T   ( n e w i d ( ) )   F O R   A u d i t I D  
 G O  
 A L T E R   T A B L E   d b o . T m p _ A u d i t   A D D   C O N S T R A I N T  
 	 D F _ A u d i t _ R e f e r e n c e I D   D E F A U L T   ( N U L L )   F O R   R e f e r e n c e I D  
 G O  
 I F   E X I S T S ( S E L E C T   *   F R O M   d b o . A u d i t )  
 	   E X E C ( ' I N S E R T   I N T O   d b o . T m p _ A u d i t   ( A u d i t I D ,   A u d i t C a t e g o r y I D ,   A u d i t C a t e g o r y N a m e ,   O r i g i n a t i o n I D ,   R e c o r d L o c a t o r ,   S t a t u s I D ,   D a t e T i m e S t a m p ,   M e s s a g e ,   Q N u m b e r ,   Q C a t e g o r y ,   P C C ,   A p p l i c a t i o n I D ,   R e f e r e n c e I D )  
 	 	 S E L E C T   A u d i t I D ,   A u d i t C a t e g o r y I D ,   A u d i t C a t e g o r y N a m e ,   O r i g i n a t i o n I D ,   R e c o r d L o c a t o r ,   S t a t u s I D ,   D a t e T i m e S t a m p ,   M e s s a g e ,   Q N u m b e r ,   Q C a t e g o r y ,   P C C ,   A p p l i c a t i o n I D ,   C O N V E R T ( n v a r c h a r ( 2 5 5 ) ,   R e f e r e n c e I D )   F R O M   d b o . A u d i t   W I T H   ( H O L D L O C K   T A B L O C K X ) ' )  
 G O  
 D R O P   T A B L E   d b o . A u d i t  
 G O  
 E X E C U T E   s p _ r e n a m e   N ' d b o . T m p _ A u d i t ' ,   N ' A u d i t ' ,   ' O B J E C T '    
 G O  
 A L T E R   T A B L E   d b o . A u d i t   A D D   C O N S T R A I N T  
 	 P K _ A u d i t   P R I M A R Y   K E Y   C L U S T E R E D    
 	 (  
 	 A u d i t I D  
 	 )   W I T H (   S T A T I S T I C S _ N O R E C O M P U T E   =   O F F ,   I G N O R E _ D U P _ K E Y   =   O F F ,   A L L O W _ R O W _ L O C K S   =   O N ,   A L L O W _ P A G E _ L O C K S   =   O N )   O N   [ P R I M A R Y ]  
  
 G O  
 A L T E R   T A B L E   d b o . A u d i t   A D D   C O N S T R A I N T  
 	 F K _ A u d i t _ A u d i t C a t e g o r y   F O R E I G N   K E Y  
 	 (  
 	 A u d i t C a t e g o r y I D ,  
 	 A u d i t C a t e g o r y N a m e  
 	 )   R E F E R E N C E S   d b o . A u d i t C a t e g o r y  
 	 (  
 	 A u d i t C a t e g o r y I D ,  
 	 A u d i t C a t e g o r y N a m e  
 	 )   O N   U P D A T E     N O   A C T I O N    
 	   O N   D E L E T E     N O   A C T I O N    
 	  
 G O  
 A L T E R   T A B L E   d b o . A u d i t   A D D   C O N S T R A I N T  
 	 F K _ A u d i t _ O r i g i n a t i o n   F O R E I G N   K E Y  
 	 (  
 	 O r i g i n a t i o n I D  
 	 )   R E F E R E N C E S   d b o . O r i g i n a t i o n  
 	 (  
 	 O r i g i n a t i o n I D  
 	 )   O N   U P D A T E     N O   A C T I O N    
 	   O N   D E L E T E     N O   A C T I O N    
 	  
 G O  
 A L T E R   T A B L E   d b o . A u d i t   A D D   C O N S T R A I N T  
 	 F K _ A u d i t _ S t a t u s   F O R E I G N   K E Y  
 	 (  
 	 S t a t u s I D  
 	 )   R E F E R E N C E S   d b o . S t a t u s  
 	 (  
 	 S t a t u s I D  
 	 )   O N   U P D A T E     N O   A C T I O N    
 	   O N   D E L E T E     N O   A C T I O N    
 	  
 G O  
 A L T E R   T A B L E   d b o . A u d i t   A D D   C O N S T R A I N T  
 	 F K _ A u d i t _ A p p l i c a t i o n   F O R E I G N   K E Y  
 	 (  
 	 A p p l i c a t i o n I D  
 	 )   R E F E R E N C E S   d b o . A p p l i c a t i o n  
 	 (  
 	 A p p l i c a t i o n I D  
 	 )   O N   U P D A T E     N O   A C T I O N    
 	   O N   D E L E T E     N O   A C T I O N    
 	  
 G O  
 C O M M I T  
 