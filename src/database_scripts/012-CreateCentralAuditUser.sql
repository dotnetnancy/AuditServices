� �  
  
 U S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ]  
 G O  
 / * * * * * *   O b j e c t :     U s e r   [ G e n e r i c R u l e s U s e r ]         S c r i p t   D a t e :   1 2 / 0 7 / 2 0 1 0   1 3 : 1 1 : 5 7   * * * * * * /  
  
 C R E A T E   L O G I N   [ G e n e r i c A u d i t U s e r ]    
         W I T H   P A S S W O R D   =   ' y o u r   p a s s w o r d ' ;  
 U S E   [ D o t N e t N a n c y C e n t r a l A u d i t S t o r e ] ;  
 C R E A T E   U S E R   [ G e n e r i c A u d i t U s e r ]   F O R   L O G I N   [ G e n e r i c A u d i t U s e r ]    
         W I T H   D E F A U L T _ S C H E M A   =   d b o ;  
 G O  
  
  
  
  
 