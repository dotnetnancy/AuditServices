� � U S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]  
 G O  
 / * * * * * *   O b j e c t :     S t o r e d P r o c e d u r e   [ d b o ] . [ G e t A u d i t C a t e g o r y ]         S c r i p t   D a t e :   1 2 / 0 7 / 2 0 1 0   1 5 : 1 1 : 1 8   * * * * * * /  
 I F     E X I S T S   ( S E L E C T   *   F R O M   s y s . o b j e c t s   W H E R E   o b j e c t _ i d   =   O B J E C T _ I D ( N ' [ d b o ] . [ G e t A u d i t C a t e g o r y ] ' )   A N D   t y p e   i n   ( N ' P ' ,   N ' P C ' ) )  
 D R O P   P R O C E D U R E   [ d b o ] . [ G e t A u d i t C a t e g o r y ]  
 G O  
  
  
 S E T   A N S I _ N U L L S   O F F  
 G O  
 S E T   Q U O T E D _ I D E N T I F I E R   O F F  
 G O  
 C R E A T E   P R O C E D U R E   [ d b o ] . [ G e t A u d i t C a t e g o r y ]  
 @ A u d i t C a t e g o r y N a m e   n v a r c h a r ( 2 5 5 ) ,  
 @ A u d i t C a t e g o r y I D   u n i q u e i d e n t i f i e r   o u t p u t ,  
 @ A u d i t C a t e g o r y D e s c r i p t i o n   n v a r c h a r ( 1 0 2 4 )   =   ' A u d i t   C a t e g o r y   D e s c r i p t i o n   N o t   P r o v i d e d ,   a u t o m a t e d   P r o c e s s '  
 A S  
  
 i f   e x i s t s (  
 S e l e c t   A u d i t C a t e g o r y I D  
 F r o m   A u d i t C a t e g o r y    
 w h e r e   A u d i t C a t e g o r y N a m e   =   @ A u d i t C a t e g o r y N a m e )  
 	  
 	 b e g i n  
  
 	 s e t   @ A u d i t C a t e g o r y I D   =   ( S e l e c t   A u d i t C a t e g o r y I D  
 	 F r o m   A u d i t C a t e g o r y    
 	 w h e r e   A u d i t C a t e g o r y N a m e   =   @ A u d i t C a t e g o r y N a m e )  
 	  
 	 e n d  
  
 e l s e  
 	 b e g i n  
 	 	 s e t   @ A u d i t C a t e g o r y I D   =   n e w i d ( ) ;  
 	 	 i n s e r t     A u d i t C a t e g o r y   ( A u d i t C a t e g o r y I D ,   A u d i t C a t e g o r y N a m e ,   A u d i t C a t e g o r y D e s c r i p t i o n )  
 	 	 v a l u e s (   @ A u d i t C a t e g o r y I D ,   @ A u d i t C a t e g o r y N a m e ,   @ A u d i t C a t e g o r y D e s c r i p t i o n )  
 	 	  
 	 e n d  
  
  
 