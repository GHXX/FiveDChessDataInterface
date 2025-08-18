
<Structures>
  <Structure Name="unnamed structure" AutoFill="0" AutoCreate="1" DefaultHex="0" AutoDestroy="0" DoNotSaveLocal="0" RLECompression="1" AutoCreateStructsize="4096">
    <Elements>
      <Element Offset="0" Vartype="4 Bytes" Bytesize="4" Description="boardId" DisplayMethod="Signed Integer"/>
      <Element Offset="4" Vartype="4 Bytes" Bytesize="4" Description="timeline" DisplayMethod="Signed Integer"/>
      <Element Offset="8" Vartype="4 Bytes" Bytesize="4" Description="turn" DisplayMethod="Signed Integer"/>
      <Element Offset="12" Vartype="4 Bytes" Bytesize="4" Description="isBlacksMove" DisplayMethod="Signed Integer"/>
      <Element Offset="16" Vartype="Array of byte" Bytesize="128" Description="positiondata" DisplayMethod="Unsigned Integer"/>
      <Element Offset="144" Vartype="4 Bytes" Bytesize="4" Description="moveTurn" DisplayMethod="Signed Integer"/>
      <Element Offset="148" Vartype="Byte" Bytesize="1" Description="moveType" DisplayMethod="Signed Integer"/>
      <Element Offset="149" Vartype="Byte" Bytesize="1" Description="bval1" DisplayMethod="Signed Integer"/>
      <Element Offset="150" Vartype="Byte" Bytesize="1" Description="bval2" DisplayMethod="Signed Integer"/>
      <Element Offset="151" Vartype="Byte" Bytesize="1" Description="bval3" DisplayMethod="Signed Integer"/>
      <Element Offset="152" Vartype="4 Bytes" Bytesize="4" Description="moveSourceL" DisplayMethod="Signed Integer"/>
      <Element Offset="156" Vartype="4 Bytes" Bytesize="4" Description="moveSourceT" DisplayMethod="Signed Integer"/>
      <Element Offset="160" Vartype="4 Bytes" Bytesize="4" Description="moveSourceIsBlack" DisplayMethod="Signed Integer"/>
      <Element Offset="164" Vartype="4 Bytes" Bytesize="4" Description="moveSourceY" DisplayMethod="Signed Integer"/>
      <Element Offset="168" Vartype="4 Bytes" Bytesize="4" Description="moveSourceX" DisplayMethod="Signed Integer"/>
      <Element Offset="172" Vartype="4 Bytes" Bytesize="4" Description="moveDestL" DisplayMethod="Signed Integer"/>
      <Element Offset="176" Vartype="4 Bytes" Bytesize="4" Description="moveDestT" DisplayMethod="Signed Integer"/>
      <Element Offset="180" Vartype="4 Bytes" Bytesize="4" Description="moveDestIsBlack" DisplayMethod="Signed Integer"/>
      <Element Offset="184" Vartype="4 Bytes" Bytesize="4" Description="moveDestY" DisplayMethod="Signed Integer"/>
      <Element Offset="188" Vartype="4 Bytes" Bytesize="4" Description="moveDestX" DisplayMethod="Signed Integer"/>
      <Element Offset="192" Vartype="4 Bytes" Bytesize="4" Description="creatingMoveNumber" DisplayMethod="Signed Integer"/>
      <Element Offset="196" Vartype="4 Bytes" Bytesize="4" Description="nextInTimelineBoardId" DisplayMethod="Signed Integer"/>
      <Element Offset="200" Vartype="4 Bytes" Bytesize="4" Description="previousBoardId" DisplayMethod="Signed Integer"/>
      <Element Offset="204" Vartype="4 Bytes" Bytesize="4" Description="createdBoardId" DisplayMethod="Signed Integer"/>
      <Element Offset="208" Vartype="4 Bytes" Bytesize="4" Description="ttPieceOriginBoardId" DisplayMethod="Signed Integer"/>
      <Element Offset="212" Vartype="4 Bytes" Bytesize="4" Description="moveSourcePosY" DisplayMethod="Signed Integer"/>
      <Element Offset="216" Vartype="4 Bytes" Bytesize="4" Description="moveSourcePosX" DisplayMethod="Signed Integer"/>
      <Element Offset="220" Vartype="4 Bytes" Bytesize="4" Description="moveDestPosY" DisplayMethod="Signed Integer"/>
      <Element Offset="224" Vartype="4 Bytes" Bytesize="4" Description="moveDestPosX" DisplayMethod="Signed Integer"/>
    </Elements>
  </Structure>
</Structures>