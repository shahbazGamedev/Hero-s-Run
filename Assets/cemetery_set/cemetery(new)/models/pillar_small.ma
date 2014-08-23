//Maya ASCII 2011 scene
//Name: pillar_small.ma
//Last modified: Tue, Aug 19, 2014 10:42:15 AM
//Codeset: 1252
requires maya "2011";
currentUnit -l centimeter -a degree -t film;
fileInfo "application" "maya";
fileInfo "product" "Maya 2011";
fileInfo "version" "2011 x64";
fileInfo "cutIdentifier" "201003190311-771506";
fileInfo "osv" "Microsoft Windows 7 Home Premium Edition, 64-bit Windows 7 Service Pack 1 (Build 7601)\n";
createNode transform -s -n "persp";
	setAttr ".v" no;
	setAttr ".t" -type "double3" 0.84261655869180885 163.9244986774134 423.41400749385502 ;
	setAttr ".r" -type "double3" -9.3383527297234217 -358.99999999995418 1.2425934254440355e-017 ;
createNode camera -s -n "perspShape" -p "persp";
	setAttr -k off ".v" no;
	setAttr ".fl" 34.999999999999993;
	setAttr ".coi" 423.09390057803279;
	setAttr ".imn" -type "string" "persp";
	setAttr ".den" -type "string" "persp_depth";
	setAttr ".man" -type "string" "persp_mask";
	setAttr ".hc" -type "string" "viewSet -p %camera";
createNode transform -s -n "top";
	setAttr ".v" no;
	setAttr ".t" -type "double3" 0 100.1 0 ;
	setAttr ".r" -type "double3" -89.999999999999986 0 0 ;
createNode camera -s -n "topShape" -p "top";
	setAttr -k off ".v" no;
	setAttr ".rnd" no;
	setAttr ".coi" 100.1;
	setAttr ".ow" 30;
	setAttr ".imn" -type "string" "top";
	setAttr ".den" -type "string" "top_depth";
	setAttr ".man" -type "string" "top_mask";
	setAttr ".hc" -type "string" "viewSet -t %camera";
	setAttr ".o" yes;
createNode transform -s -n "front";
	setAttr ".v" no;
	setAttr ".t" -type "double3" 0 0 100.1 ;
createNode camera -s -n "frontShape" -p "front";
	setAttr -k off ".v" no;
	setAttr ".rnd" no;
	setAttr ".coi" 100.1;
	setAttr ".ow" 30;
	setAttr ".imn" -type "string" "front";
	setAttr ".den" -type "string" "front_depth";
	setAttr ".man" -type "string" "front_mask";
	setAttr ".hc" -type "string" "viewSet -f %camera";
	setAttr ".o" yes;
createNode transform -s -n "side";
	setAttr ".v" no;
	setAttr ".t" -type "double3" 100.1 0 0 ;
	setAttr ".r" -type "double3" 0 89.999999999999986 0 ;
createNode camera -s -n "sideShape" -p "side";
	setAttr -k off ".v" no;
	setAttr ".rnd" no;
	setAttr ".coi" 100.1;
	setAttr ".ow" 30;
	setAttr ".imn" -type "string" "side";
	setAttr ".den" -type "string" "side_depth";
	setAttr ".man" -type "string" "side_mask";
	setAttr ".hc" -type "string" "viewSet -s %camera";
	setAttr ".o" yes;
createNode transform -n "cemetery_fence_04_corner_left_mobile";
createNode mesh -n "cemetery_fence_04_corner_left_mobileShape" -p "cemetery_fence_04_corner_left_mobile";
	addAttr -ci true -sn "mso" -ln "miShadingSamplesOverride" -min 0 -max 1 -at "bool";
	addAttr -ci true -sn "msh" -ln "miShadingSamples" -min 0 -smx 8 -at "float";
	addAttr -ci true -sn "mdo" -ln "miMaxDisplaceOverride" -min 0 -max 1 -at "bool";
	addAttr -ci true -sn "mmd" -ln "miMaxDisplace" -min 0 -smx 1 -at "float";
	setAttr -k off ".v";
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr -s 87 ".uvst[0].uvsp[0:86]" -type "float2" 0.9375037 0.010816634 
		0.91221392 0.010816634 0.91221392 0.0084689856 0.9375037 0.0084689856 0.90948278 
		0.013166875 0.91221356 0.013166875 0.91221064 0.01551795 0.90948278 0.01551795 0.9375034 
		0.013166875 0.91221392 0.0061221719 0.9375037 0.0061221719 0.90948278 0.0084689856 
		0.90948278 0.010816634 0.90633404 0.010844827 0.90633404 0.0084689856 0.90633404 
		0.013166875 0.90633404 0.01551795 0.90948278 0.0061221719 0.90633404 0.0061221719 
		0.89693546 0.010868818 0.89693546 0.0084689856 0.89693487 0.013166875 0.89693451 
		0.01551795 0.89693546 0.0061221719 0.89393193 0.0084689856 0.89393193 0.010893047 
		0.89111495 0.010275483 0.89111495 0.0090630352 0.89393115 0.013166875 0.89111435 
		0.012565851 0.89111495 0.011429787 0.8939302 0.01551795 0.89111227 0.014843941 0.8911128 
		0.013667703 0.89393193 0.0061221719 0.89111495 0.0077639818 0.89111495 0.0065900385 
		0.93749738 0.01551795 0.88556921 0.0096572042 0.88556921 0.011966228 0.88556623 0.014168918 
		0.88556921 0.0070587993 0.70701647 0.055307955 0.70706767 0.087920696 0.70503259 
		0.085849077 0.70499194 0.057392329 0.67495143 0.087971687 0.67697358 0.085889339 
		0.67490411 0.055354655 0.67693293 0.057432741 0.70299983 0.083821774 0.70299983 0.059460759 
		0.6789645 0.083821774 0.6789645 0.059460759 0.54928732 0.050619543 0.52684212 0.050619543 
		0.57158065 0.050619543 0.5944854 0.050619543 0.50476885 0.050619543 0.54928917 0.053055733 
		0.54928917 0.055890381 0.52684671 0.055890381 0.52684671 0.053055733 0.57155442 0.053055733 
		0.57155442 0.055890381 0.59347504 0.053055733 0.59347504 0.055890381 0.50476885 0.055890381 
		0.50476885 0.053055733 0.54928732 0.047302485 0.52684212 0.047302485 0.52684212 0.026857704 
		0.54928732 0.026857704 0.57158065 0.047302485 0.57158065 0.026857704 0.5944854 0.047302485 
		0.5944854 0.026857704 0.50476885 0.047302485 0.50476885 0.026857704 0.69956565 0.079904437 
		0.69956565 0.063377917 0.68239915 0.079904437 0.68239915 0.063377917 0.7580387 0.094040811 
		0.78273267 0.094132647 0.78263879 0.11882342 0.75794494 0.11873157;
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr -s 61 ".pt[0:60]" -type "float3"  -1.0786539 0 -5.8156476 -1.0786539 
		0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 
		-1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 
		0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 
		-1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 
		0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 
		-1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 
		0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 
		-1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -1.0786539 0 -5.8156476 -13.264385 
		0 -12.890799 -13.264385 0 12.905653 13.264385 0 12.905653 13.264385 0 -12.890799 
		13.04341 0 -12.905653 13.045044 0 12.865355 13.19431 0 -12.878916 -13.194305 0 -12.878916 
		13.19431 0 12.893785 -13.194305 0 12.893785 13.029517 0 -12.891953 -12.976851 0 -12.891953 
		13.031148 0 12.851634 -13.028168 0 12.851634 8.2717228 0 -8.0211754 -8.2300463 0 
		-8.0211754 8.2717228 0 8.0554914 -8.2300463 0 8.0554914 -8.358758 0 -8.1466932 -8.358758 
		0 8.1810102 8.4004364 0 -8.1466932 8.4004364 0 8.1810102 10.343381 0 -10.040865 -10.3017 
		0 -10.040865 10.343381 0 10.075144 -10.3017 0 10.075144 9.8407269 0 -9.623868 -9.7990522 
		0 -9.623868 9.8407269 0 9.6581841 -9.7990522 0 9.6581841 -12.990735 0 -12.905653 
		-13.042054 0 12.865355;
	setAttr -s 61 ".vt[0:60]"  0.95760727 81.648415 10.410919 -4.0918541 
		81.648415 5.6842351 -3.5151024 122.42925 5.6842351 0.95760727 122.42925 9.8710632 
		12.128107 125.87875 5.6841741 0.95760727 125.87875 -4.7723079 0.95760727 122.42925 
		1.4973454 5.4303246 122.42925 5.6842351 0.95760727 81.648415 0.95748949 6.0070915 
		81.648415 5.6842351 0.95760727 125.87875 16.140717 -10.212849 125.87875 5.6841741 
		-3.5257149 128.89154 5.6842351 0.95760727 128.89154 9.8809509 0.95760727 128.89154 
		1.4874578 5.4409294 128.89154 5.6842351 -11.865772 152.54059 5.6841741 0.95759964 
		152.54059 17.687897 0.95759964 152.54059 -6.3196101 13.781008 152.54059 5.6841741 
		0.95760727 174.15245 5.6842351 -3.3992119 147.67117 5.6842351 0.95760727 147.67117 
		9.7626038 0.95760727 147.67117 1.605927 5.3144341 147.67117 5.6842351 -5.4540901 
		155.84328 5.6841741 0.95760727 155.84328 11.686066 0.95760727 155.84328 -0.31765699 
		7.3693123 155.84328 5.6841741 33.03767 -0.4730292 31.494354 33.03767 -0.4730292 -33.445484 
		-32.93399 -0.4730292 -33.445484 -32.93399 -0.4730292 31.494354 -32.384468 87.77903 
		31.53175 -32.388527 87.77903 -33.34404 -32.759727 7.45259 31.464447 32.863396 7.45259 
		31.464447 -32.759727 7.45259 -33.415607 32.863396 7.45259 -33.415607 -32.349915 78.630081 
		31.497265 32.322628 78.630081 31.497265 -32.353973 78.630081 -33.309494 32.450241 
		78.630081 -33.309494 -20.518251 19.74662 19.235596 20.518293 19.74662 19.235596 -20.518251 
		19.74662 -21.235718 20.518293 19.74662 -21.235718 20.838369 66.263405 19.551575 20.838369 
		66.263405 -21.551697 -20.838335 66.263405 19.551575 -20.838335 66.263405 -21.551697 
		-25.670046 11.216231 24.319946 25.670073 11.216231 24.319946 -25.670046 11.216231 
		-26.319977 25.670073 11.216231 -26.319977 -24.420046 74.314011 23.270203 24.420088 
		74.314011 23.270203 -24.420046 74.314011 -25.270325 24.420088 74.314011 -25.270325 
		32.357155 87.77903 31.53175 32.484772 87.77903 -33.34404;
	setAttr -s 161 ".ed[0:160]"  1 3 0 3 2 0 
		2 1 0 1 0 0 0 3 0 5 7 0 
		7 6 0 6 5 0 5 4 0 4 7 0 
		8 2 0 2 6 0 6 8 0 8 1 0 
		0 7 0 7 3 0 0 9 0 9 7 0 
		10 12 0 12 11 0 11 10 0 10 13 0 
		13 12 0 11 14 0 14 5 0 5 11 0 
		12 14 0 5 15 0 15 4 0 14 15 0 
		4 13 0 10 4 0 15 13 0 12 22 0 
		22 21 0 21 12 0 13 22 0 12 23 0 
		23 14 0 21 23 0 15 23 0 23 24 0 
		24 15 0 15 22 0 24 22 0 17 25 0 
		25 16 0 16 17 0 17 26 0 26 25 0 
		18 25 0 25 27 0 27 18 0 18 16 0 
		19 27 0 27 28 0 28 19 0 19 18 0 
		19 26 0 17 19 0 28 26 0 11 3 0 
		3 10 0 11 2 0 8 7 0 9 8 0 
		5 2 0 10 7 0 21 17 0 16 21 0 
		22 17 0 18 21 0 18 23 0 24 18 0 
		19 24 0 17 24 0 26 20 0 20 25 0 
		20 27 0 20 28 0 29 35 0 35 32 0 
		32 29 0 29 36 0 36 35 0 32 37 0 
		37 31 0 31 32 0 35 37 0 31 38 0 
		38 30 0 30 31 0 37 38 0 30 36 0 
		29 30 0 38 36 0 35 52 0 52 51 0 
		51 35 0 36 52 0 37 51 0 51 53 0 
		53 37 0 38 53 0 53 54 0 54 38 0 
		36 54 0 54 52 0 39 59 0 59 33 0 
		33 39 0 39 40 0 40 59 0 41 33 0 
		33 34 0 34 41 0 41 39 0 42 34 0 
		34 60 0 60 42 0 42 41 0 40 60 0 
		60 59 0 40 42 0 49 44 0 44 47 0 
		47 49 0 49 43 0 43 44 0 50 43 0 
		49 50 0 50 45 0 45 43 0 48 45 0 
		50 48 0 48 46 0 46 45 0 47 46 0 
		48 47 0 44 46 0 55 49 0 47 56 0 
		56 55 0 57 50 0 55 57 0 58 48 0 
		57 58 0 58 56 0 43 51 0 52 44 0 
		45 53 0 46 54 0 39 56 0 56 40 0 
		39 55 0 41 55 0 41 57 0 42 57 0 
		42 58 0 40 58 0 59 34 0;
	setAttr -s 314 ".n";
	setAttr ".n[0:165]" -type "float3"  -0.68335301 0.0096642962 0.73002422 
		-0.68335396 0.0096645067 0.73002332 -0.68335402 0.0096645346 0.73002326 -0.68335396 
		0.0096645067 0.73002332 -0.68335301 0.0096642962 0.73002422 -0.68335289 0.0096642692 
		0.73002434 0.41129941 -0.79860902 -0.43938196 0.41129696 -0.79861063 -0.43938136 
		0.41130492 -0.7986055 -0.43938312 0.41129696 -0.79861063 -0.43938136 0.41129941 -0.79860902 
		-0.43938196 0.4112947 -0.79861194 -0.43938088 -0.68336153 0.0096645905 -0.73001617 
		-0.68336344 0.00966419 -0.7300145 -0.68336362 0.009664136 -0.7300142 -0.68336344 
		0.00966419 -0.7300145 -0.68336153 0.0096645905 -0.73001617 -0.68336129 0.009664638 
		-0.73001647 0.68335211 0.0096646892 0.73002505 0.68335384 0.0096643073 0.73002344 
		0.68335402 0.0096642552 0.73002326 0.68335384 0.0096643073 0.73002344 0.68335211 
		0.0096646892 0.73002505 0.68335187 0.009664733 0.73002523 -0.37614495 0.83489317 
		0.40183124 -0.37614912 0.83489078 0.40183231 -0.37615255 0.83488888 0.4018333 -0.37614912 
		0.83489078 0.40183231 -0.37614495 0.83489317 0.40183124 -0.37613636 0.83489823 0.401829 
		-0.37614322 0.83489621 -0.40182677 -0.37614334 0.83489579 -0.40182728 -0.37614343 
		0.83489561 -0.40182775 -0.37614334 0.83489579 -0.40182728 -0.37614322 0.83489621 
		-0.40182677 -0.37614295 0.83489686 -0.40182558 0.37614283 0.83489633 -0.40182677 
		0.37614158 0.8348971 -0.40182644 0.37614059 0.8348977 -0.4018262 0.37614158 0.8348971 
		-0.40182644 0.37614283 0.83489633 -0.40182677 0.37614527 0.8348949 -0.40182739 0.37614706 
		0.83489102 0.40183383 0.37614632 0.83489305 0.40183035 0.37614566 0.83489478 0.40182739 
		0.37614632 0.83489305 0.40183035 0.37614706 0.83489102 0.40183383 0.37614876 0.83488643 
		0.40184161 -0.68337619 0.0046005086 0.73005199 -0.68338132 0.0046027936 0.73004711 
		-0.68338311 0.0046035554 0.7300455 -0.68338132 0.0046027936 0.73004711 -0.68337619 
		0.0046005086 0.73005199 -0.6833744 0.0045997645 0.7300536 -0.68338382 0.0046038963 
		-0.73004484 -0.68338329 0.0046036406 -0.73004532 -0.683384 0.0046039792 -0.73004466 
		-0.68338329 0.0046036406 -0.73004532 -0.68338382 0.0046038963 -0.73004484 -0.68338311 
		0.0046035554 -0.7300455 0.68338424 0.0046038446 -0.73004442 0.68338335 0.0046034399 
		-0.73004526 0.68338311 0.0046033049 -0.7300455 0.68338335 0.0046034399 -0.73004526 
		0.68338424 0.0046038446 -0.73004442 0.68338454 0.0046039764 -0.73004419 0.68337661 
		0.0046004565 0.73005158 0.68338144 0.0046025924 0.73004699 0.68337506 0.0045997612 
		0.73005307 0.68338144 0.0046025924 0.73004699 0.68337661 0.0046004565 0.73005158 
		0.68338311 0.0046033049 0.7300455 -0.41134453 0.79855996 0.43942884 -0.41134313 0.79856098 
		0.43942842 -0.4113425 0.79856133 0.43942815 -0.41134313 0.79856098 0.43942842 -0.41134453 
		0.79855996 0.43942884 -0.41134605 0.79855889 0.43942934 -0.41133827 0.79856437 -0.43942672 
		-0.41134119 0.79856235 -0.4394277 -0.41133502 0.79856664 -0.43942556 -0.41134119 
		0.79856235 -0.4394277 -0.41133827 0.79856437 -0.43942672 -0.4113425 0.79856133 -0.43942815 
		0.41133997 0.79856217 -0.43942922 0.41133937 0.7985636 -0.43942714 0.41134068 0.79856044 
		-0.43943167 0.41133937 0.7985636 -0.43942714 0.41133997 0.79856217 -0.43942922 0.41133907 
		0.79856426 -0.43942615 0.41134289 0.79856128 0.43942788 0.4113436 0.79855967 0.43943021 
		0.41134387 0.79855901 0.43943128 0.4113436 0.79855967 0.43943021 0.41134289 0.79856128 
		0.43942788 0.41134211 0.79856324 0.43942511 -0.41130391 -0.79860312 0.43938851 -0.4113034 
		-0.79860473 0.43938598 -0.41130301 -0.79860634 0.43938354 -0.4113034 -0.79860473 
		0.43938598 -0.41130391 -0.79860312 0.43938851 -0.41130501 -0.79859912 0.43939474 
		0.68336064 0.0096647851 -0.73001701 0.68336332 0.0096642133 -0.73001462 0.68336028 
		0.009664854 -0.7300173 0.68336332 0.0096642133 -0.73001462 0.68336064 0.0096647851 
		-0.73001701 0.68336362 0.009664136 -0.7300142 -0.41130042 -0.79860842 -0.43938217 
		-0.41129845 -0.79860955 -0.43938172 -0.41129667 -0.79861075 -0.43938136 -0.41129845 
		-0.79860955 -0.43938172 -0.41130042 -0.79860842 -0.43938217 -0.41130492 -0.7986055 
		-0.43938312 0.41130075 -0.79860604 0.4393861 0.41130283 -0.79860473 0.43938658 0.41130471 
		-0.79860348 0.43938696 0.41130283 -0.79860473 0.43938658 0.41130075 -0.79860604 0.4393861 
		0.41129592 -0.79860902 0.43938503 -0.44004223 -0.76510149 0.4700878 -0.44004211 -0.76510155 
		0.47008786 -0.4400427 -0.76510125 0.47008792 -0.44004211 -0.76510155 0.47008786 -0.44004223 
		-0.76510149 0.4700878 -0.44004107 -0.76510233 0.47008771 -0.44003493 -0.76511043 
		-0.4700802 -0.4400351 -0.76511025 -0.47008017 -0.44003555 -0.76511002 -0.47008029 
		-0.4400351 -0.76511025 -0.47008017 -0.44003493 -0.76511043 -0.4700802 -0.44003394 
		-0.76511103 -0.47008014 0.44003463 -0.76511008 -0.47008103 0.4400346 -0.76511008 
		-0.470081 0.44003454 -0.76511014 -0.47008103 0.4400346 -0.76511008 -0.470081 0.44003463 
		-0.76511008 -0.47008103 0.44003466 -0.76510996 -0.47008097 0.44004172 -0.76510125 
		0.4700886 0.44004175 -0.76510131 0.4700886 0.44004169 -0.76510131 0.4700886 0.44004175 
		-0.76510131 0.4700886 0.44004172 -0.76510125 0.4700886 0.4400419 -0.76510125 0.4700886 
		-0.66462672 0.23274194 0.71000183 -0.66462672 0.23274194 0.71000189 -0.66462672 0.23274194 
		0.71000183 -0.66461933 0.23274785 -0.71000689 -0.66461927 0.23274785 -0.71000689 
		-0.66461927 0.23274785 -0.71000689 0.66461927 0.23274785 -0.71000689 0.66461933 0.23274785 
		-0.71000689 0.66461927 0.23274785 -0.71000689 0.66462672 0.23274194 0.71000189 0.66462672 
		0.23274194 0.71000183 0.66462672 0.23274194 0.71000183 -2.775538e-017 0.0037703803 
		0.99999297 -2.775538e-017 0.0037703807 0.99999297 -2.775538e-017 0.0037703803 0.99999297 
		-2.775538e-017 0.0037703807 0.99999297 -2.775538e-017 0.0037703803 0.99999297 -2.7755379e-017 
		0.0037703805 0.99999291 -0.99975836 0.021982953 4.8811961e-018 -0.99975836 0.021982955 
		4.8811965e-018 -0.99975842 0.021982955 4.8811965e-018 -0.99975836 0.021982955 4.8811965e-018;
	setAttr ".n[166:313]" -type "float3"  -0.99975836 0.021982953 4.8811961e-018 
		-0.99975836 0.021982955 4.8811965e-018 2.7755379e-017 0.00377038 -0.99999291 2.775538e-017 
		0.0037703807 -0.99999297 2.7755379e-017 0.0037703805 -0.99999291 2.775538e-017 0.0037703807 
		-0.99999297 2.7755379e-017 0.00377038 -0.99999291 2.7755379e-017 0.0037703805 -0.99999291 
		0.99975836 0.021982953 4.8811961e-018 0.99975836 0.021982955 4.8811965e-018 0.99975842 
		0.021982955 4.8811965e-018 0.99975836 0.021982955 4.8811965e-018 0.99975836 0.021982953 
		4.8811961e-018 0.99975836 0.021982955 4.8811965e-018 -0.16342267 0.89519858 0.4146235 
		0.26316217 0.92754245 0.26535022 -0.2631619 0.92754257 0.26535013 0.26316217 0.92754245 
		0.26535022 -0.16342267 0.89519858 0.4146235 0.16342252 0.89519846 0.41462374 -0.41332546 
		0.89603621 -0.16211489 -0.2631619 0.92754257 0.26535013 -0.26316112 0.92754292 -0.26534966 
		-0.2631619 0.92754257 0.26535013 -0.41332546 0.89603621 -0.16211489 -0.41332799 0.89603543 
		0.1621128 0.16342068 0.89519852 -0.41462436 -0.26316112 0.92754292 -0.26534966 0.26316142 
		0.92754281 -0.26534972 -0.26316112 0.92754292 -0.26534966 0.16342068 0.89519852 -0.41462436 
		-0.16342086 0.89519864 -0.41462401 0.41332823 0.89603525 0.16211306 0.26316142 0.92754281 
		-0.26534972 0.26316217 0.92754245 0.26535022 0.26316142 0.92754281 -0.26534972 0.41332823 
		0.89603525 0.16211306 0.41332561 0.89603609 -0.16211516 -2.7755316e-017 -0.0043300986 
		0.99999064 -2.7755318e-017 -0.0043300986 0.9999907 -2.7755318e-017 -0.0043300986 
		0.9999907 -2.7755318e-017 -0.0043300986 0.9999907 -2.7755316e-017 -0.0043300986 0.99999064 
		-2.7755316e-017 -0.0043300986 0.99999064 -0.99999058 -0.0043369937 -9.6300605e-019 
		-0.99999058 -0.0043369932 -9.6300594e-019 -0.99999058 -0.0043369932 -9.6300594e-019 
		-0.99999058 -0.0043369932 -9.6300594e-019 -0.99999058 -0.0043369937 -9.6300605e-019 
		-0.99999058 -0.0043369927 -9.6300584e-019 2.7755314e-017 -0.0043438878 -0.99999058 
		2.7755314e-017 -0.0043438873 -0.99999058 2.7755314e-017 -0.0043438878 -0.99999058 
		2.7755314e-017 -0.0043438873 -0.99999058 2.7755314e-017 -0.0043438878 -0.99999058 
		2.7755314e-017 -0.0043438878 -0.99999058 0.99999058 -0.0043344074 -9.6243178e-019 
		0.99999058 -0.0043344069 -9.6243167e-019 0.99999058 -0.0043344079 -9.6243188e-019 
		0.99999058 -0.0043344069 -9.6243167e-019 0.99999058 -0.0043344074 -9.6243178e-019 
		0.99999058 -0.0043344069 -9.6243167e-019 -2.7754935e-017 -0.0067923763 0.99997693 
		-2.7754935e-017 -0.0067923754 0.99997693 -2.7754935e-017 -0.0067923763 0.99997693 
		-2.7754935e-017 -0.0067923754 0.99997693 -2.7754935e-017 -0.0067923763 0.99997693 
		-2.7754935e-017 -0.0067923763 0.99997693 -0.9999764 -0.0068809376 -1.5278751e-018 
		-0.9999764 -0.0068809381 -1.5278752e-018 -0.9999764 -0.0068809376 -1.5278751e-018 
		-0.9999764 -0.0068809381 -1.5278752e-018 -0.9999764 -0.0068809376 -1.5278751e-018 
		-0.9999764 -0.0068809381 -1.5278752e-018 2.7754935e-017 -0.0067923763 -0.99997693 
		2.7754935e-017 -0.0067923763 -0.99997693 2.7754935e-017 -0.0067923763 -0.99997693 
		2.7754935e-017 -0.0067923763 -0.99997693 2.7754935e-017 -0.0067923763 -0.99997693 
		2.7754935e-017 -0.0067923763 -0.99997693 0.9999764 -0.0068806424 -1.5278095e-018 
		0.9999764 -0.0068806428 -1.5278096e-018 0.9999764 -0.0068806424 -1.5278095e-018 0.9999764 
		-0.0068806428 -1.5278096e-018 0.9999764 -0.0068806424 -1.5278095e-018 0.9999764 -0.0068806428 
		-1.5278096e-018 -2.7576302e-017 -0.11347403 0.993541 -0.24516216 -0.93697625 0.24893998 
		0.2451621 -0.93697625 0.24894011 -2.7576302e-017 -0.11347403 0.993541 -0.99623942 
		-0.086643666 -1.9238759e-017 -0.2451621 -0.93697625 -0.24894011 -0.24516216 -0.93697625 
		0.24893998 -0.99623942 -0.086643673 -1.923876e-017 2.7576302e-017 -0.11347403 -0.993541 
		0.24516216 -0.93697625 -0.24893998 -0.2451621 -0.93697625 -0.24894011 2.7576302e-017 
		-0.11347403 -0.993541 0.99623924 -0.086645767 -1.9239225e-017 0.2451621 -0.93697625 
		0.24894011 0.24516216 -0.93697625 -0.24893998 0.99623924 -0.086645767 -1.9239225e-017 
		-0.21556276 0.95160651 0.21903819 -2.7375969e-017 0.16482267 0.98632324 -2.7375969e-017 
		0.16482267 0.98632324 0.2155636 0.95160645 0.21903802 -0.21556279 0.95160651 -0.21903814 
		-0.98595786 0.16699387 3.7080088e-017 -0.98595798 0.16699392 3.7080098e-017 -0.21556276 
		0.95160651 0.21903819 0.21556358 0.95160639 -0.21903805 2.7376042e-017 0.16480695 
		-0.98632586 2.7376044e-017 0.16480695 -0.98632592 -0.21556279 0.95160651 -0.21903814 
		0.2155636 0.95160645 0.21903802 0.98595798 0.1669939 3.7080095e-017 0.98595786 0.16699392 
		3.7080098e-017 0.21556358 0.95160639 -0.21903805 -0.16575059 -0.89689535 0.41000649 
		0.2662603 -0.92778057 0.26139751 0.16575062 -0.89689547 0.41000634 0.2662603 -0.92778057 
		0.26139751 -0.16575059 -0.89689535 0.41000649 -0.26626053 -0.92778057 0.26139748 
		-0.41251358 -0.8952325 -0.1684972 -0.26626053 -0.92778057 0.26139748 -0.41251436 
		-0.89523256 0.1684954 -0.26626053 -0.92778057 0.26139748 -0.41251358 -0.8952325 -0.1684972 
		-0.26626 -0.92777979 -0.26140064 0.16575065 -0.8968935 -0.41001076 -0.26626 -0.92777979 
		-0.26140064 -0.16575064 -0.8968935 -0.41001084 -0.26626 -0.92777979 -0.26140064 0.16575065 
		-0.8968935 -0.41001076 0.26625979 -0.92777985 -0.26140064 0.41251409 -0.89523268 
		0.16849528 0.26625979 -0.92777985 -0.26140064 0.41251338 -0.89523262 -0.1684971 0.26625979 
		-0.92777985 -0.26140064 0.41251409 -0.89523268 0.16849528 0.2662603 -0.92778057 0.26139751 
		0.0011991874 0.94548708 0.32565752 -8.5758173e-018 0.95106977 0.30897638 -0.32680649 
		0.94509131 2.0985243e-016 9.0707038e-018 0.94509137 -0.32680655 -0.32680649 0.94509131 
		2.0985243e-016 -8.5758173e-018 0.95106977 0.30897638;
	setAttr -s 102 ".fc[0:101]" -type "polyFaces" 
		f 3 0 1 2 
		mu 0 3 0 2 1 
		f 3 -1 3 4 
		mu 0 3 2 0 3 
		f 3 5 6 7 
		mu 0 3 4 6 5 
		f 3 -6 8 9 
		mu 0 3 6 4 7 
		f 3 10 11 12 
		mu 0 3 8 1 5 
		f 3 -11 13 -3 
		mu 0 3 1 8 0 
		f 3 14 15 -5 
		mu 0 3 3 9 2 
		f 3 -15 16 17 
		mu 0 3 9 3 10 
		f 3 18 19 20 
		mu 0 3 11 13 12 
		f 3 -19 21 22 
		mu 0 3 13 11 14 
		f 3 23 24 25 
		mu 0 3 12 15 4 
		f 3 -24 -20 26 
		mu 0 3 15 12 13 
		f 3 27 28 -9 
		mu 0 3 4 16 7 
		f 3 -28 -25 29 
		mu 0 3 16 4 15 
		f 3 30 -22 31 
		mu 0 3 17 14 11 
		f 3 -31 -29 32 
		mu 0 3 14 17 18 
		f 3 33 34 35 
		mu 0 3 13 20 19 
		f 3 -34 -23 36 
		mu 0 3 20 13 14 
		f 3 37 38 -27 
		mu 0 3 13 21 15 
		f 3 -38 -36 39 
		mu 0 3 21 13 19 
		f 3 40 41 42 
		mu 0 3 16 21 22 
		f 3 -41 -30 -39 
		mu 0 3 21 16 15 
		f 3 43 -37 -33 
		mu 0 3 18 20 14 
		f 3 -44 -43 44 
		mu 0 3 20 18 23 
		f 3 45 46 47 
		mu 0 3 24 26 25 
		f 3 -46 48 49 
		mu 0 3 26 24 27 
		f 3 50 51 52 
		mu 0 3 28 30 29 
		f 3 -51 53 -47 
		mu 0 3 30 28 25 
		f 3 54 55 56 
		mu 0 3 31 33 32 
		f 3 -55 57 -53 
		mu 0 3 33 31 28 
		f 3 58 -49 59 
		mu 0 3 34 35 24 
		f 3 -59 -57 60 
		mu 0 3 35 34 36 
		f 3 61 62 -21 
		mu 0 3 12 2 11 
		f 3 -62 63 -2 
		mu 0 3 2 12 1 
		f 3 64 -18 65 
		mu 0 3 8 6 37 
		f 3 -65 -13 -7 
		mu 0 3 6 8 5 
		f 3 66 -64 -26 
		mu 0 3 4 1 12 
		f 3 -67 -8 -12 
		mu 0 3 1 4 5 
		f 3 67 -10 -32 
		mu 0 3 11 9 17 
		f 3 -68 -63 -16 
		mu 0 3 9 11 2 
		f 3 68 -48 69 
		mu 0 3 19 24 25 
		f 3 -69 -35 70 
		mu 0 3 24 19 20 
		f 3 71 -70 -54 
		mu 0 3 28 19 25 
		f 3 -72 72 -40 
		mu 0 3 19 28 21 
		f 3 73 -58 74 
		mu 0 3 22 28 31 
		f 3 -74 -42 -73 
		mu 0 3 28 22 21 
		f 3 75 -75 -60 
		mu 0 3 24 23 34 
		f 3 -76 -71 -45 
		mu 0 3 23 24 20 
		f 3 -50 76 77 
		mu 0 3 26 27 38 
		f 3 -52 -78 78 
		mu 0 3 29 30 39 
		f 3 -56 -79 79 
		mu 0 3 32 33 40 
		f 3 -61 -80 -77 
		mu 0 3 35 36 41 
		f 3 80 81 82 
		mu 0 3 42 44 43 
		f 3 -81 83 84 
		mu 0 3 44 42 45 
		f 3 85 86 87 
		mu 0 3 43 47 46 
		f 3 -86 -82 88 
		mu 0 3 47 43 44 
		f 3 89 90 91 
		mu 0 3 46 49 48 
		f 3 -90 -87 92 
		mu 0 3 49 46 47 
		f 3 93 -84 94 
		mu 0 3 48 45 42 
		f 3 -94 -91 95 
		mu 0 3 45 48 49 
		f 3 96 97 98 
		mu 0 3 44 51 50 
		f 3 -97 -85 99 
		mu 0 3 51 44 45 
		f 3 100 101 102 
		mu 0 3 47 50 52 
		f 3 -101 -89 -99 
		mu 0 3 50 47 44 
		f 3 103 104 105 
		mu 0 3 49 52 53 
		f 3 -104 -93 -103 
		mu 0 3 52 49 47 
		f 3 106 107 -100 
		mu 0 3 45 53 51 
		f 3 -107 -96 -106 
		mu 0 3 53 45 49 
		f 3 108 109 110 
		mu 0 3 59 61 60 
		f 3 -109 111 112 
		mu 0 3 61 59 62 
		f 3 113 114 115 
		mu 0 3 63 60 64 
		f 3 -114 116 -111 
		mu 0 3 60 63 59 
		f 3 117 118 119 
		mu 0 3 65 64 66 
		f 3 -118 120 -116 
		mu 0 3 64 65 63 
		f 3 121 122 -113 
		mu 0 3 62 67 61 
		f 3 -122 123 -120 
		mu 0 3 67 62 68 
		f 3 124 125 126 
		mu 0 3 69 71 70 
		f 3 -125 127 128 
		mu 0 3 71 69 72 
		f 3 129 -128 130 
		mu 0 3 73 72 69 
		f 3 -130 131 132 
		mu 0 3 72 73 74 
		f 3 133 -132 134 
		mu 0 3 75 74 73 
		f 3 -134 135 136 
		mu 0 3 74 75 76 
		f 3 137 -136 138 
		mu 0 3 70 78 77 
		f 3 -138 -126 139 
		mu 0 3 78 70 71 
		f 4 140 -127 141 142 
		mu 0 4 54 69 70 55 
		f 4 143 -131 -141 144 
		mu 0 4 56 73 69 54 
		f 4 145 -135 -144 146 
		mu 0 4 57 75 73 56 
		f 4 -142 -139 -146 147 
		mu 0 4 55 70 77 58 
		f 4 148 -98 149 -129 
		mu 0 4 79 50 51 80 
		f 4 150 -102 -149 -133 
		mu 0 4 81 52 50 79 
		f 4 151 -105 -151 -137 
		mu 0 4 82 53 52 81 
		f 4 -150 -108 -152 -140 
		mu 0 4 80 51 53 82 
		f 3 152 153 -112 
		mu 0 3 59 55 62 
		f 3 -153 154 -143 
		mu 0 3 55 59 54 
		f 3 155 -155 -117 
		mu 0 3 63 54 59 
		f 3 -156 156 -145 
		mu 0 3 54 63 56 
		f 3 157 -157 -121 
		mu 0 3 65 56 63 
		f 3 -158 158 -147 
		mu 0 3 56 65 57 
		f 3 159 -159 -124 
		mu 0 3 62 58 68 
		f 3 -160 -154 -148 
		mu 0 3 58 62 55 
		f 3 -110 160 -115 
		mu 0 3 84 83 85 
		f 3 -119 -161 -123 
		mu 0 3 86 85 83 ;
	setAttr ".cd" -type "dataPolyComponent" Index_Data Edge 0 ;
	setAttr ".cvd" -type "dataPolyComponent" Index_Data Vertex 0 ;
	setAttr ".hfd" -type "dataPolyComponent" Index_Data Face 0 ;
createNode lightLinker -s -n "lightLinker1";
	setAttr -s 4 ".lnk";
	setAttr -s 4 ".slnk";
createNode displayLayerManager -n "layerManager";
	setAttr -s 2 ".dli[1]"  1;
	setAttr -s 2 ".dli";
createNode displayLayer -n "defaultLayer";
createNode renderLayerManager -n "renderLayerManager";
createNode renderLayer -n "defaultRenderLayer";
	setAttr ".g" yes;
createNode lambert -n "cemetery_set_atlas";
	setAttr ".ambc" -type "float3" 1 1 1 ;
createNode shadingEngine -n "cemetery_fence_04_corner_left_mobileSG";
	setAttr ".ihi" 0;
	setAttr ".ro" yes;
createNode materialInfo -n "materialInfo1";
createNode file -n "cemetery_01_atlas";
	setAttr ".ftn" -type "string" "C:/Users/mattira/Documents/My Dropbox/BITGEM_products/_Cemetery_Set/cemetery_set_01_lp/textures/cemetery_set_01_atlas_4096.tga";
createNode place2dTexture -n "place2dTexture1";
createNode lambert -n "cemetery_set_atlas_transp";
	setAttr ".ambc" -type "float3" 1 1 1 ;
createNode shadingEngine -n "transpSG";
	setAttr ".ihi" 0;
	setAttr ".ro" yes;
createNode materialInfo -n "materialInfo2";
createNode file -n "pasted__cemetery_01_atlas";
	setAttr ".ftn" -type "string" "C:/Users/mattira/Documents/My Dropbox/BITGEM_products/_Cemetery_Set/cemetery_set_01_lp/textures/cemetery_set_01_atlas_4096.tga";
createNode place2dTexture -n "place2dTexture2";
createNode displayLayer -n "cemetery_set_mobile";
	setAttr ".do" 1;
createNode script -n "uiConfigurationScriptNode";
	setAttr ".b" -type "string" (
		"// Maya Mel UI Configuration File.\n//\n//  This script is machine generated.  Edit at your own risk.\n//\n//\n\nglobal string $gMainPane;\nif (`paneLayout -exists $gMainPane`) {\n\n\tglobal int $gUseScenePanelConfig;\n\tint    $useSceneConfig = $gUseScenePanelConfig;\n\tint    $menusOkayInPanels = `optionVar -q allowMenusInPanels`;\tint    $nVisPanes = `paneLayout -q -nvp $gMainPane`;\n\tint    $nPanes = 0;\n\tstring $editorName;\n\tstring $panelName;\n\tstring $itemFilterName;\n\tstring $panelConfig;\n\n\t//\n\t//  get current state of the UI\n\t//\n\tsceneUIReplacement -update $gMainPane;\n\n\t$panelName = `sceneUIReplacement -getNextPanel \"modelPanel\" (localizedPanelLabel(\"Top View\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `modelPanel -unParent -l (localizedPanelLabel(\"Top View\")) -mbv $menusOkayInPanels `;\n\t\t\t$editorName = $panelName;\n            modelEditor -e \n                -camera \"top\" \n                -useInteractiveMode 0\n                -displayLights \"default\" \n                -displayAppearance \"wireframe\" \n"
		+ "                -activeOnly 0\n                -ignorePanZoom 0\n                -wireframeOnShaded 0\n                -headsUpDisplay 1\n                -selectionHiliteDisplay 1\n                -useDefaultMaterial 0\n                -bufferMode \"double\" \n                -twoSidedLighting 1\n                -backfaceCulling 0\n                -xray 0\n                -jointXray 0\n                -activeComponentsXray 0\n                -displayTextures 0\n                -smoothWireframe 0\n                -lineWidth 1\n                -textureAnisotropic 0\n                -textureHilight 1\n                -textureSampling 2\n                -textureDisplay \"modulate\" \n                -textureMaxSize 16384\n                -fogging 0\n                -fogSource \"fragment\" \n                -fogMode \"linear\" \n                -fogStart 0\n                -fogEnd 100\n                -fogDensity 0.1\n                -fogColor 0.5 0.5 0.5 1 \n                -maxConstantTransparency 1\n                -rendererName \"base_OpenGL_Renderer\" \n"
		+ "                -colorResolution 256 256 \n                -bumpResolution 512 512 \n                -textureCompression 0\n                -transparencyAlgorithm \"frontAndBackCull\" \n                -transpInShadows 0\n                -cullingOverride \"none\" \n                -lowQualityLighting 0\n                -maximumNumHardwareLights 1\n                -occlusionCulling 0\n                -shadingModel 0\n                -useBaseRenderer 0\n                -useReducedRenderer 0\n                -smallObjectCulling 0\n                -smallObjectThreshold -1 \n                -interactiveDisableShadows 0\n                -interactiveBackFaceCull 0\n                -sortTransparent 1\n                -nurbsCurves 1\n                -nurbsSurfaces 1\n                -polymeshes 1\n                -subdivSurfaces 1\n                -planes 1\n                -lights 1\n                -cameras 1\n                -controlVertices 1\n                -hulls 1\n                -grid 1\n                -joints 1\n                -ikHandles 1\n"
		+ "                -deformers 1\n                -dynamics 1\n                -fluids 1\n                -hairSystems 1\n                -follicles 1\n                -nCloths 1\n                -nParticles 1\n                -nRigids 1\n                -dynamicConstraints 1\n                -locators 1\n                -manipulators 1\n                -dimensions 1\n                -handles 1\n                -pivots 1\n                -textures 1\n                -strokes 1\n                -shadows 0\n                $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tmodelPanel -edit -l (localizedPanelLabel(\"Top View\")) -mbv $menusOkayInPanels  $panelName;\n\t\t$editorName = $panelName;\n        modelEditor -e \n            -camera \"top\" \n            -useInteractiveMode 0\n            -displayLights \"default\" \n            -displayAppearance \"wireframe\" \n            -activeOnly 0\n            -ignorePanZoom 0\n            -wireframeOnShaded 0\n            -headsUpDisplay 1\n            -selectionHiliteDisplay 1\n"
		+ "            -useDefaultMaterial 0\n            -bufferMode \"double\" \n            -twoSidedLighting 1\n            -backfaceCulling 0\n            -xray 0\n            -jointXray 0\n            -activeComponentsXray 0\n            -displayTextures 0\n            -smoothWireframe 0\n            -lineWidth 1\n            -textureAnisotropic 0\n            -textureHilight 1\n            -textureSampling 2\n            -textureDisplay \"modulate\" \n            -textureMaxSize 16384\n            -fogging 0\n            -fogSource \"fragment\" \n            -fogMode \"linear\" \n            -fogStart 0\n            -fogEnd 100\n            -fogDensity 0.1\n            -fogColor 0.5 0.5 0.5 1 \n            -maxConstantTransparency 1\n            -rendererName \"base_OpenGL_Renderer\" \n            -colorResolution 256 256 \n            -bumpResolution 512 512 \n            -textureCompression 0\n            -transparencyAlgorithm \"frontAndBackCull\" \n            -transpInShadows 0\n            -cullingOverride \"none\" \n            -lowQualityLighting 0\n"
		+ "            -maximumNumHardwareLights 1\n            -occlusionCulling 0\n            -shadingModel 0\n            -useBaseRenderer 0\n            -useReducedRenderer 0\n            -smallObjectCulling 0\n            -smallObjectThreshold -1 \n            -interactiveDisableShadows 0\n            -interactiveBackFaceCull 0\n            -sortTransparent 1\n            -nurbsCurves 1\n            -nurbsSurfaces 1\n            -polymeshes 1\n            -subdivSurfaces 1\n            -planes 1\n            -lights 1\n            -cameras 1\n            -controlVertices 1\n            -hulls 1\n            -grid 1\n            -joints 1\n            -ikHandles 1\n            -deformers 1\n            -dynamics 1\n            -fluids 1\n            -hairSystems 1\n            -follicles 1\n            -nCloths 1\n            -nParticles 1\n            -nRigids 1\n            -dynamicConstraints 1\n            -locators 1\n            -manipulators 1\n            -dimensions 1\n            -handles 1\n            -pivots 1\n            -textures 1\n            -strokes 1\n"
		+ "            -shadows 0\n            $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextPanel \"modelPanel\" (localizedPanelLabel(\"Side View\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `modelPanel -unParent -l (localizedPanelLabel(\"Side View\")) -mbv $menusOkayInPanels `;\n\t\t\t$editorName = $panelName;\n            modelEditor -e \n                -camera \"side\" \n                -useInteractiveMode 0\n                -displayLights \"default\" \n                -displayAppearance \"wireframe\" \n                -activeOnly 0\n                -ignorePanZoom 0\n                -wireframeOnShaded 0\n                -headsUpDisplay 1\n                -selectionHiliteDisplay 1\n                -useDefaultMaterial 0\n                -bufferMode \"double\" \n                -twoSidedLighting 1\n                -backfaceCulling 0\n                -xray 0\n                -jointXray 0\n                -activeComponentsXray 0\n"
		+ "                -displayTextures 0\n                -smoothWireframe 0\n                -lineWidth 1\n                -textureAnisotropic 0\n                -textureHilight 1\n                -textureSampling 2\n                -textureDisplay \"modulate\" \n                -textureMaxSize 16384\n                -fogging 0\n                -fogSource \"fragment\" \n                -fogMode \"linear\" \n                -fogStart 0\n                -fogEnd 100\n                -fogDensity 0.1\n                -fogColor 0.5 0.5 0.5 1 \n                -maxConstantTransparency 1\n                -rendererName \"base_OpenGL_Renderer\" \n                -colorResolution 256 256 \n                -bumpResolution 512 512 \n                -textureCompression 0\n                -transparencyAlgorithm \"frontAndBackCull\" \n                -transpInShadows 0\n                -cullingOverride \"none\" \n                -lowQualityLighting 0\n                -maximumNumHardwareLights 1\n                -occlusionCulling 0\n                -shadingModel 0\n                -useBaseRenderer 0\n"
		+ "                -useReducedRenderer 0\n                -smallObjectCulling 0\n                -smallObjectThreshold -1 \n                -interactiveDisableShadows 0\n                -interactiveBackFaceCull 0\n                -sortTransparent 1\n                -nurbsCurves 1\n                -nurbsSurfaces 1\n                -polymeshes 1\n                -subdivSurfaces 1\n                -planes 1\n                -lights 1\n                -cameras 1\n                -controlVertices 1\n                -hulls 1\n                -grid 1\n                -joints 1\n                -ikHandles 1\n                -deformers 1\n                -dynamics 1\n                -fluids 1\n                -hairSystems 1\n                -follicles 1\n                -nCloths 1\n                -nParticles 1\n                -nRigids 1\n                -dynamicConstraints 1\n                -locators 1\n                -manipulators 1\n                -dimensions 1\n                -handles 1\n                -pivots 1\n                -textures 1\n                -strokes 1\n"
		+ "                -shadows 0\n                $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tmodelPanel -edit -l (localizedPanelLabel(\"Side View\")) -mbv $menusOkayInPanels  $panelName;\n\t\t$editorName = $panelName;\n        modelEditor -e \n            -camera \"side\" \n            -useInteractiveMode 0\n            -displayLights \"default\" \n            -displayAppearance \"wireframe\" \n            -activeOnly 0\n            -ignorePanZoom 0\n            -wireframeOnShaded 0\n            -headsUpDisplay 1\n            -selectionHiliteDisplay 1\n            -useDefaultMaterial 0\n            -bufferMode \"double\" \n            -twoSidedLighting 1\n            -backfaceCulling 0\n            -xray 0\n            -jointXray 0\n            -activeComponentsXray 0\n            -displayTextures 0\n            -smoothWireframe 0\n            -lineWidth 1\n            -textureAnisotropic 0\n            -textureHilight 1\n            -textureSampling 2\n            -textureDisplay \"modulate\" \n"
		+ "            -textureMaxSize 16384\n            -fogging 0\n            -fogSource \"fragment\" \n            -fogMode \"linear\" \n            -fogStart 0\n            -fogEnd 100\n            -fogDensity 0.1\n            -fogColor 0.5 0.5 0.5 1 \n            -maxConstantTransparency 1\n            -rendererName \"base_OpenGL_Renderer\" \n            -colorResolution 256 256 \n            -bumpResolution 512 512 \n            -textureCompression 0\n            -transparencyAlgorithm \"frontAndBackCull\" \n            -transpInShadows 0\n            -cullingOverride \"none\" \n            -lowQualityLighting 0\n            -maximumNumHardwareLights 1\n            -occlusionCulling 0\n            -shadingModel 0\n            -useBaseRenderer 0\n            -useReducedRenderer 0\n            -smallObjectCulling 0\n            -smallObjectThreshold -1 \n            -interactiveDisableShadows 0\n            -interactiveBackFaceCull 0\n            -sortTransparent 1\n            -nurbsCurves 1\n            -nurbsSurfaces 1\n            -polymeshes 1\n            -subdivSurfaces 1\n"
		+ "            -planes 1\n            -lights 1\n            -cameras 1\n            -controlVertices 1\n            -hulls 1\n            -grid 1\n            -joints 1\n            -ikHandles 1\n            -deformers 1\n            -dynamics 1\n            -fluids 1\n            -hairSystems 1\n            -follicles 1\n            -nCloths 1\n            -nParticles 1\n            -nRigids 1\n            -dynamicConstraints 1\n            -locators 1\n            -manipulators 1\n            -dimensions 1\n            -handles 1\n            -pivots 1\n            -textures 1\n            -strokes 1\n            -shadows 0\n            $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextPanel \"modelPanel\" (localizedPanelLabel(\"Front View\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `modelPanel -unParent -l (localizedPanelLabel(\"Front View\")) -mbv $menusOkayInPanels `;\n\t\t\t$editorName = $panelName;\n"
		+ "            modelEditor -e \n                -camera \"front\" \n                -useInteractiveMode 0\n                -displayLights \"default\" \n                -displayAppearance \"wireframe\" \n                -activeOnly 0\n                -ignorePanZoom 0\n                -wireframeOnShaded 0\n                -headsUpDisplay 1\n                -selectionHiliteDisplay 1\n                -useDefaultMaterial 0\n                -bufferMode \"double\" \n                -twoSidedLighting 1\n                -backfaceCulling 0\n                -xray 0\n                -jointXray 0\n                -activeComponentsXray 0\n                -displayTextures 0\n                -smoothWireframe 0\n                -lineWidth 1\n                -textureAnisotropic 0\n                -textureHilight 1\n                -textureSampling 2\n                -textureDisplay \"modulate\" \n                -textureMaxSize 16384\n                -fogging 0\n                -fogSource \"fragment\" \n                -fogMode \"linear\" \n                -fogStart 0\n                -fogEnd 100\n"
		+ "                -fogDensity 0.1\n                -fogColor 0.5 0.5 0.5 1 \n                -maxConstantTransparency 1\n                -rendererName \"base_OpenGL_Renderer\" \n                -colorResolution 256 256 \n                -bumpResolution 512 512 \n                -textureCompression 0\n                -transparencyAlgorithm \"frontAndBackCull\" \n                -transpInShadows 0\n                -cullingOverride \"none\" \n                -lowQualityLighting 0\n                -maximumNumHardwareLights 1\n                -occlusionCulling 0\n                -shadingModel 0\n                -useBaseRenderer 0\n                -useReducedRenderer 0\n                -smallObjectCulling 0\n                -smallObjectThreshold -1 \n                -interactiveDisableShadows 0\n                -interactiveBackFaceCull 0\n                -sortTransparent 1\n                -nurbsCurves 1\n                -nurbsSurfaces 1\n                -polymeshes 1\n                -subdivSurfaces 1\n                -planes 1\n                -lights 1\n"
		+ "                -cameras 1\n                -controlVertices 1\n                -hulls 1\n                -grid 1\n                -joints 1\n                -ikHandles 1\n                -deformers 1\n                -dynamics 1\n                -fluids 1\n                -hairSystems 1\n                -follicles 1\n                -nCloths 1\n                -nParticles 1\n                -nRigids 1\n                -dynamicConstraints 1\n                -locators 1\n                -manipulators 1\n                -dimensions 1\n                -handles 1\n                -pivots 1\n                -textures 1\n                -strokes 1\n                -shadows 0\n                $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tmodelPanel -edit -l (localizedPanelLabel(\"Front View\")) -mbv $menusOkayInPanels  $panelName;\n\t\t$editorName = $panelName;\n        modelEditor -e \n            -camera \"front\" \n            -useInteractiveMode 0\n            -displayLights \"default\" \n"
		+ "            -displayAppearance \"wireframe\" \n            -activeOnly 0\n            -ignorePanZoom 0\n            -wireframeOnShaded 0\n            -headsUpDisplay 1\n            -selectionHiliteDisplay 1\n            -useDefaultMaterial 0\n            -bufferMode \"double\" \n            -twoSidedLighting 1\n            -backfaceCulling 0\n            -xray 0\n            -jointXray 0\n            -activeComponentsXray 0\n            -displayTextures 0\n            -smoothWireframe 0\n            -lineWidth 1\n            -textureAnisotropic 0\n            -textureHilight 1\n            -textureSampling 2\n            -textureDisplay \"modulate\" \n            -textureMaxSize 16384\n            -fogging 0\n            -fogSource \"fragment\" \n            -fogMode \"linear\" \n            -fogStart 0\n            -fogEnd 100\n            -fogDensity 0.1\n            -fogColor 0.5 0.5 0.5 1 \n            -maxConstantTransparency 1\n            -rendererName \"base_OpenGL_Renderer\" \n            -colorResolution 256 256 \n            -bumpResolution 512 512 \n"
		+ "            -textureCompression 0\n            -transparencyAlgorithm \"frontAndBackCull\" \n            -transpInShadows 0\n            -cullingOverride \"none\" \n            -lowQualityLighting 0\n            -maximumNumHardwareLights 1\n            -occlusionCulling 0\n            -shadingModel 0\n            -useBaseRenderer 0\n            -useReducedRenderer 0\n            -smallObjectCulling 0\n            -smallObjectThreshold -1 \n            -interactiveDisableShadows 0\n            -interactiveBackFaceCull 0\n            -sortTransparent 1\n            -nurbsCurves 1\n            -nurbsSurfaces 1\n            -polymeshes 1\n            -subdivSurfaces 1\n            -planes 1\n            -lights 1\n            -cameras 1\n            -controlVertices 1\n            -hulls 1\n            -grid 1\n            -joints 1\n            -ikHandles 1\n            -deformers 1\n            -dynamics 1\n            -fluids 1\n            -hairSystems 1\n            -follicles 1\n            -nCloths 1\n            -nParticles 1\n            -nRigids 1\n"
		+ "            -dynamicConstraints 1\n            -locators 1\n            -manipulators 1\n            -dimensions 1\n            -handles 1\n            -pivots 1\n            -textures 1\n            -strokes 1\n            -shadows 0\n            $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextPanel \"modelPanel\" (localizedPanelLabel(\"Persp View\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `modelPanel -unParent -l (localizedPanelLabel(\"Persp View\")) -mbv $menusOkayInPanels `;\n\t\t\t$editorName = $panelName;\n            modelEditor -e \n                -camera \"persp\" \n                -useInteractiveMode 0\n                -displayLights \"default\" \n                -displayAppearance \"smoothShaded\" \n                -activeOnly 0\n                -ignorePanZoom 0\n                -wireframeOnShaded 0\n                -headsUpDisplay 1\n                -selectionHiliteDisplay 1\n                -useDefaultMaterial 0\n"
		+ "                -bufferMode \"double\" \n                -twoSidedLighting 1\n                -backfaceCulling 0\n                -xray 0\n                -jointXray 0\n                -activeComponentsXray 0\n                -displayTextures 0\n                -smoothWireframe 0\n                -lineWidth 1\n                -textureAnisotropic 0\n                -textureHilight 1\n                -textureSampling 2\n                -textureDisplay \"modulate\" \n                -textureMaxSize 16384\n                -fogging 0\n                -fogSource \"fragment\" \n                -fogMode \"linear\" \n                -fogStart 0\n                -fogEnd 100\n                -fogDensity 0.1\n                -fogColor 0.5 0.5 0.5 1 \n                -maxConstantTransparency 1\n                -rendererName \"base_OpenGL_Renderer\" \n                -colorResolution 256 256 \n                -bumpResolution 512 512 \n                -textureCompression 0\n                -transparencyAlgorithm \"frontAndBackCull\" \n                -transpInShadows 0\n"
		+ "                -cullingOverride \"none\" \n                -lowQualityLighting 0\n                -maximumNumHardwareLights 1\n                -occlusionCulling 0\n                -shadingModel 0\n                -useBaseRenderer 0\n                -useReducedRenderer 0\n                -smallObjectCulling 0\n                -smallObjectThreshold -1 \n                -interactiveDisableShadows 0\n                -interactiveBackFaceCull 0\n                -sortTransparent 1\n                -nurbsCurves 1\n                -nurbsSurfaces 1\n                -polymeshes 1\n                -subdivSurfaces 1\n                -planes 1\n                -lights 1\n                -cameras 1\n                -controlVertices 1\n                -hulls 1\n                -grid 1\n                -joints 1\n                -ikHandles 1\n                -deformers 1\n                -dynamics 1\n                -fluids 1\n                -hairSystems 1\n                -follicles 1\n                -nCloths 1\n                -nParticles 1\n                -nRigids 1\n"
		+ "                -dynamicConstraints 1\n                -locators 1\n                -manipulators 1\n                -dimensions 1\n                -handles 1\n                -pivots 1\n                -textures 1\n                -strokes 1\n                -shadows 0\n                $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tmodelPanel -edit -l (localizedPanelLabel(\"Persp View\")) -mbv $menusOkayInPanels  $panelName;\n\t\t$editorName = $panelName;\n        modelEditor -e \n            -camera \"persp\" \n            -useInteractiveMode 0\n            -displayLights \"default\" \n            -displayAppearance \"smoothShaded\" \n            -activeOnly 0\n            -ignorePanZoom 0\n            -wireframeOnShaded 0\n            -headsUpDisplay 1\n            -selectionHiliteDisplay 1\n            -useDefaultMaterial 0\n            -bufferMode \"double\" \n            -twoSidedLighting 1\n            -backfaceCulling 0\n            -xray 0\n            -jointXray 0\n            -activeComponentsXray 0\n"
		+ "            -displayTextures 0\n            -smoothWireframe 0\n            -lineWidth 1\n            -textureAnisotropic 0\n            -textureHilight 1\n            -textureSampling 2\n            -textureDisplay \"modulate\" \n            -textureMaxSize 16384\n            -fogging 0\n            -fogSource \"fragment\" \n            -fogMode \"linear\" \n            -fogStart 0\n            -fogEnd 100\n            -fogDensity 0.1\n            -fogColor 0.5 0.5 0.5 1 \n            -maxConstantTransparency 1\n            -rendererName \"base_OpenGL_Renderer\" \n            -colorResolution 256 256 \n            -bumpResolution 512 512 \n            -textureCompression 0\n            -transparencyAlgorithm \"frontAndBackCull\" \n            -transpInShadows 0\n            -cullingOverride \"none\" \n            -lowQualityLighting 0\n            -maximumNumHardwareLights 1\n            -occlusionCulling 0\n            -shadingModel 0\n            -useBaseRenderer 0\n            -useReducedRenderer 0\n            -smallObjectCulling 0\n            -smallObjectThreshold -1 \n"
		+ "            -interactiveDisableShadows 0\n            -interactiveBackFaceCull 0\n            -sortTransparent 1\n            -nurbsCurves 1\n            -nurbsSurfaces 1\n            -polymeshes 1\n            -subdivSurfaces 1\n            -planes 1\n            -lights 1\n            -cameras 1\n            -controlVertices 1\n            -hulls 1\n            -grid 1\n            -joints 1\n            -ikHandles 1\n            -deformers 1\n            -dynamics 1\n            -fluids 1\n            -hairSystems 1\n            -follicles 1\n            -nCloths 1\n            -nParticles 1\n            -nRigids 1\n            -dynamicConstraints 1\n            -locators 1\n            -manipulators 1\n            -dimensions 1\n            -handles 1\n            -pivots 1\n            -textures 1\n            -strokes 1\n            -shadows 0\n            $editorName;\nmodelEditor -e -viewSelected 0 $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextPanel \"outlinerPanel\" (localizedPanelLabel(\"Outliner\")) `;\n"
		+ "\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `outlinerPanel -unParent -l (localizedPanelLabel(\"Outliner\")) -mbv $menusOkayInPanels `;\n\t\t\t$editorName = $panelName;\n            outlinerEditor -e \n                -showShapes 0\n                -showAttributes 0\n                -showConnected 0\n                -showAnimCurvesOnly 0\n                -showMuteInfo 0\n                -organizeByLayer 1\n                -showAnimLayerWeight 1\n                -autoExpandLayers 1\n                -autoExpand 0\n                -showDagOnly 1\n                -showAssets 1\n                -showContainedOnly 1\n                -showPublishedAsConnected 0\n                -showContainerContents 1\n                -ignoreDagHierarchy 0\n                -expandConnections 0\n                -showUpstreamCurves 1\n                -showUnitlessCurves 1\n                -showCompounds 1\n                -showLeafs 1\n                -showNumericAttrsOnly 0\n                -highlightActive 1\n                -autoSelectNewObjects 0\n"
		+ "                -doNotSelectNewObjects 0\n                -dropIsParent 1\n                -transmitFilters 0\n                -setFilter \"defaultSetFilter\" \n                -showSetMembers 1\n                -allowMultiSelection 1\n                -alwaysToggleSelect 0\n                -directSelect 0\n                -displayMode \"DAG\" \n                -expandObjects 0\n                -setsIgnoreFilters 1\n                -containersIgnoreFilters 0\n                -editAttrName 0\n                -showAttrValues 0\n                -highlightSecondary 0\n                -showUVAttrsOnly 0\n                -showTextureNodesOnly 0\n                -attrAlphaOrder \"default\" \n                -animLayerFilterOptions \"allAffecting\" \n                -sortOrder \"none\" \n                -longNames 0\n                -niceNames 1\n                -showNamespace 1\n                -showPinIcons 0\n                $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\toutlinerPanel -edit -l (localizedPanelLabel(\"Outliner\")) -mbv $menusOkayInPanels  $panelName;\n"
		+ "\t\t$editorName = $panelName;\n        outlinerEditor -e \n            -showShapes 0\n            -showAttributes 0\n            -showConnected 0\n            -showAnimCurvesOnly 0\n            -showMuteInfo 0\n            -organizeByLayer 1\n            -showAnimLayerWeight 1\n            -autoExpandLayers 1\n            -autoExpand 0\n            -showDagOnly 1\n            -showAssets 1\n            -showContainedOnly 1\n            -showPublishedAsConnected 0\n            -showContainerContents 1\n            -ignoreDagHierarchy 0\n            -expandConnections 0\n            -showUpstreamCurves 1\n            -showUnitlessCurves 1\n            -showCompounds 1\n            -showLeafs 1\n            -showNumericAttrsOnly 0\n            -highlightActive 1\n            -autoSelectNewObjects 0\n            -doNotSelectNewObjects 0\n            -dropIsParent 1\n            -transmitFilters 0\n            -setFilter \"defaultSetFilter\" \n            -showSetMembers 1\n            -allowMultiSelection 1\n            -alwaysToggleSelect 0\n            -directSelect 0\n"
		+ "            -displayMode \"DAG\" \n            -expandObjects 0\n            -setsIgnoreFilters 1\n            -containersIgnoreFilters 0\n            -editAttrName 0\n            -showAttrValues 0\n            -highlightSecondary 0\n            -showUVAttrsOnly 0\n            -showTextureNodesOnly 0\n            -attrAlphaOrder \"default\" \n            -animLayerFilterOptions \"allAffecting\" \n            -sortOrder \"none\" \n            -longNames 0\n            -niceNames 1\n            -showNamespace 1\n            -showPinIcons 0\n            $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"graphEditor\" (localizedPanelLabel(\"Graph Editor\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"graphEditor\" -l (localizedPanelLabel(\"Graph Editor\")) -mbv $menusOkayInPanels `;\n\n\t\t\t$editorName = ($panelName+\"OutlineEd\");\n            outlinerEditor -e \n                -showShapes 1\n                -showAttributes 1\n"
		+ "                -showConnected 1\n                -showAnimCurvesOnly 1\n                -showMuteInfo 0\n                -organizeByLayer 1\n                -showAnimLayerWeight 1\n                -autoExpandLayers 1\n                -autoExpand 1\n                -showDagOnly 0\n                -showAssets 1\n                -showContainedOnly 0\n                -showPublishedAsConnected 0\n                -showContainerContents 0\n                -ignoreDagHierarchy 0\n                -expandConnections 1\n                -showUpstreamCurves 1\n                -showUnitlessCurves 1\n                -showCompounds 0\n                -showLeafs 1\n                -showNumericAttrsOnly 1\n                -highlightActive 0\n                -autoSelectNewObjects 1\n                -doNotSelectNewObjects 0\n                -dropIsParent 1\n                -transmitFilters 1\n                -setFilter \"0\" \n                -showSetMembers 0\n                -allowMultiSelection 1\n                -alwaysToggleSelect 0\n                -directSelect 0\n"
		+ "                -displayMode \"DAG\" \n                -expandObjects 0\n                -setsIgnoreFilters 1\n                -containersIgnoreFilters 0\n                -editAttrName 0\n                -showAttrValues 0\n                -highlightSecondary 0\n                -showUVAttrsOnly 0\n                -showTextureNodesOnly 0\n                -attrAlphaOrder \"default\" \n                -animLayerFilterOptions \"allAffecting\" \n                -sortOrder \"none\" \n                -longNames 0\n                -niceNames 1\n                -showNamespace 1\n                -showPinIcons 1\n                $editorName;\n\n\t\t\t$editorName = ($panelName+\"GraphEd\");\n            animCurveEditor -e \n                -displayKeys 1\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 1\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"integer\" \n                -snapValue \"none\" \n                -showResults \"off\" \n                -showBufferCurves \"off\" \n"
		+ "                -smoothness \"fine\" \n                -resultSamples 1\n                -resultScreenSamples 0\n                -resultUpdate \"delayed\" \n                -showUpstreamCurves 1\n                -stackedCurves 0\n                -stackedCurvesMin -1\n                -stackedCurvesMax 1\n                -stackedCurvesSpace 0.2\n                -displayNormalized 0\n                -preSelectionHighlight 0\n                -constrainDrag 0\n                $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Graph Editor\")) -mbv $menusOkayInPanels  $panelName;\n\n\t\t\t$editorName = ($panelName+\"OutlineEd\");\n            outlinerEditor -e \n                -showShapes 1\n                -showAttributes 1\n                -showConnected 1\n                -showAnimCurvesOnly 1\n                -showMuteInfo 0\n                -organizeByLayer 1\n                -showAnimLayerWeight 1\n                -autoExpandLayers 1\n                -autoExpand 1\n                -showDagOnly 0\n"
		+ "                -showAssets 1\n                -showContainedOnly 0\n                -showPublishedAsConnected 0\n                -showContainerContents 0\n                -ignoreDagHierarchy 0\n                -expandConnections 1\n                -showUpstreamCurves 1\n                -showUnitlessCurves 1\n                -showCompounds 0\n                -showLeafs 1\n                -showNumericAttrsOnly 1\n                -highlightActive 0\n                -autoSelectNewObjects 1\n                -doNotSelectNewObjects 0\n                -dropIsParent 1\n                -transmitFilters 1\n                -setFilter \"0\" \n                -showSetMembers 0\n                -allowMultiSelection 1\n                -alwaysToggleSelect 0\n                -directSelect 0\n                -displayMode \"DAG\" \n                -expandObjects 0\n                -setsIgnoreFilters 1\n                -containersIgnoreFilters 0\n                -editAttrName 0\n                -showAttrValues 0\n                -highlightSecondary 0\n                -showUVAttrsOnly 0\n"
		+ "                -showTextureNodesOnly 0\n                -attrAlphaOrder \"default\" \n                -animLayerFilterOptions \"allAffecting\" \n                -sortOrder \"none\" \n                -longNames 0\n                -niceNames 1\n                -showNamespace 1\n                -showPinIcons 1\n                $editorName;\n\n\t\t\t$editorName = ($panelName+\"GraphEd\");\n            animCurveEditor -e \n                -displayKeys 1\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 1\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"integer\" \n                -snapValue \"none\" \n                -showResults \"off\" \n                -showBufferCurves \"off\" \n                -smoothness \"fine\" \n                -resultSamples 1\n                -resultScreenSamples 0\n                -resultUpdate \"delayed\" \n                -showUpstreamCurves 1\n                -stackedCurves 0\n                -stackedCurvesMin -1\n                -stackedCurvesMax 1\n"
		+ "                -stackedCurvesSpace 0.2\n                -displayNormalized 0\n                -preSelectionHighlight 0\n                -constrainDrag 0\n                $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"dopeSheetPanel\" (localizedPanelLabel(\"Dope Sheet\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"dopeSheetPanel\" -l (localizedPanelLabel(\"Dope Sheet\")) -mbv $menusOkayInPanels `;\n\n\t\t\t$editorName = ($panelName+\"OutlineEd\");\n            outlinerEditor -e \n                -showShapes 1\n                -showAttributes 1\n                -showConnected 1\n                -showAnimCurvesOnly 1\n                -showMuteInfo 0\n                -organizeByLayer 1\n                -showAnimLayerWeight 1\n                -autoExpandLayers 1\n                -autoExpand 0\n                -showDagOnly 0\n                -showAssets 1\n                -showContainedOnly 0\n                -showPublishedAsConnected 0\n"
		+ "                -showContainerContents 0\n                -ignoreDagHierarchy 0\n                -expandConnections 1\n                -showUpstreamCurves 1\n                -showUnitlessCurves 0\n                -showCompounds 1\n                -showLeafs 1\n                -showNumericAttrsOnly 1\n                -highlightActive 0\n                -autoSelectNewObjects 0\n                -doNotSelectNewObjects 1\n                -dropIsParent 1\n                -transmitFilters 0\n                -setFilter \"0\" \n                -showSetMembers 0\n                -allowMultiSelection 1\n                -alwaysToggleSelect 0\n                -directSelect 0\n                -displayMode \"DAG\" \n                -expandObjects 0\n                -setsIgnoreFilters 1\n                -containersIgnoreFilters 0\n                -editAttrName 0\n                -showAttrValues 0\n                -highlightSecondary 0\n                -showUVAttrsOnly 0\n                -showTextureNodesOnly 0\n                -attrAlphaOrder \"default\" \n                -animLayerFilterOptions \"allAffecting\" \n"
		+ "                -sortOrder \"none\" \n                -longNames 0\n                -niceNames 1\n                -showNamespace 1\n                -showPinIcons 0\n                $editorName;\n\n\t\t\t$editorName = ($panelName+\"DopeSheetEd\");\n            dopeSheetEditor -e \n                -displayKeys 1\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 0\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"integer\" \n                -snapValue \"none\" \n                -outliner \"dopeSheetPanel1OutlineEd\" \n                -showSummary 1\n                -showScene 0\n                -hierarchyBelow 0\n                -showTicks 1\n                -selectionWindow 0 0 0 0 \n                $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Dope Sheet\")) -mbv $menusOkayInPanels  $panelName;\n\n\t\t\t$editorName = ($panelName+\"OutlineEd\");\n            outlinerEditor -e \n                -showShapes 1\n"
		+ "                -showAttributes 1\n                -showConnected 1\n                -showAnimCurvesOnly 1\n                -showMuteInfo 0\n                -organizeByLayer 1\n                -showAnimLayerWeight 1\n                -autoExpandLayers 1\n                -autoExpand 0\n                -showDagOnly 0\n                -showAssets 1\n                -showContainedOnly 0\n                -showPublishedAsConnected 0\n                -showContainerContents 0\n                -ignoreDagHierarchy 0\n                -expandConnections 1\n                -showUpstreamCurves 1\n                -showUnitlessCurves 0\n                -showCompounds 1\n                -showLeafs 1\n                -showNumericAttrsOnly 1\n                -highlightActive 0\n                -autoSelectNewObjects 0\n                -doNotSelectNewObjects 1\n                -dropIsParent 1\n                -transmitFilters 0\n                -setFilter \"0\" \n                -showSetMembers 0\n                -allowMultiSelection 1\n                -alwaysToggleSelect 0\n"
		+ "                -directSelect 0\n                -displayMode \"DAG\" \n                -expandObjects 0\n                -setsIgnoreFilters 1\n                -containersIgnoreFilters 0\n                -editAttrName 0\n                -showAttrValues 0\n                -highlightSecondary 0\n                -showUVAttrsOnly 0\n                -showTextureNodesOnly 0\n                -attrAlphaOrder \"default\" \n                -animLayerFilterOptions \"allAffecting\" \n                -sortOrder \"none\" \n                -longNames 0\n                -niceNames 1\n                -showNamespace 1\n                -showPinIcons 0\n                $editorName;\n\n\t\t\t$editorName = ($panelName+\"DopeSheetEd\");\n            dopeSheetEditor -e \n                -displayKeys 1\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 0\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"integer\" \n                -snapValue \"none\" \n                -outliner \"dopeSheetPanel1OutlineEd\" \n"
		+ "                -showSummary 1\n                -showScene 0\n                -hierarchyBelow 0\n                -showTicks 1\n                -selectionWindow 0 0 0 0 \n                $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"clipEditorPanel\" (localizedPanelLabel(\"Trax Editor\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"clipEditorPanel\" -l (localizedPanelLabel(\"Trax Editor\")) -mbv $menusOkayInPanels `;\n\n\t\t\t$editorName = clipEditorNameFromPanel($panelName);\n            clipEditor -e \n                -displayKeys 0\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 0\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"none\" \n                -snapValue \"none\" \n                -manageSequencer 0 \n                $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n"
		+ "\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Trax Editor\")) -mbv $menusOkayInPanels  $panelName;\n\n\t\t\t$editorName = clipEditorNameFromPanel($panelName);\n            clipEditor -e \n                -displayKeys 0\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 0\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"none\" \n                -snapValue \"none\" \n                -manageSequencer 0 \n                $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"sequenceEditorPanel\" (localizedPanelLabel(\"Camera Sequencer\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"sequenceEditorPanel\" -l (localizedPanelLabel(\"Camera Sequencer\")) -mbv $menusOkayInPanels `;\n\n\t\t\t$editorName = sequenceEditorNameFromPanel($panelName);\n            clipEditor -e \n                -displayKeys 0\n"
		+ "                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 0\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"none\" \n                -snapValue \"none\" \n                -manageSequencer 1 \n                $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Camera Sequencer\")) -mbv $menusOkayInPanels  $panelName;\n\n\t\t\t$editorName = sequenceEditorNameFromPanel($panelName);\n            clipEditor -e \n                -displayKeys 0\n                -displayTangents 0\n                -displayActiveKeys 0\n                -displayActiveKeyTangents 0\n                -displayInfinities 0\n                -autoFit 0\n                -snapTime \"none\" \n                -snapValue \"none\" \n                -manageSequencer 1 \n                $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"hyperGraphPanel\" (localizedPanelLabel(\"Hypergraph Hierarchy\")) `;\n"
		+ "\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"hyperGraphPanel\" -l (localizedPanelLabel(\"Hypergraph Hierarchy\")) -mbv $menusOkayInPanels `;\n\n\t\t\t$editorName = ($panelName+\"HyperGraphEd\");\n            hyperGraph -e \n                -graphLayoutStyle \"hierarchicalLayout\" \n                -orientation \"horiz\" \n                -mergeConnections 0\n                -zoom 1\n                -animateTransition 0\n                -showRelationships 1\n                -showShapes 0\n                -showDeformers 0\n                -showExpressions 0\n                -showConstraints 0\n                -showUnderworld 0\n                -showInvisible 0\n                -transitionFrames 1\n                -opaqueContainers 0\n                -freeform 0\n                -imagePosition 0 0 \n                -imageScale 1\n                -imageEnabled 0\n                -graphType \"DAG\" \n                -heatMapDisplay 0\n                -updateSelection 1\n                -updateNodeAdded 1\n"
		+ "                -useDrawOverrideColor 0\n                -limitGraphTraversal -1\n                -range 0 0 \n                -iconSize \"smallIcons\" \n                -showCachedConnections 0\n                $editorName;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Hypergraph Hierarchy\")) -mbv $menusOkayInPanels  $panelName;\n\n\t\t\t$editorName = ($panelName+\"HyperGraphEd\");\n            hyperGraph -e \n                -graphLayoutStyle \"hierarchicalLayout\" \n                -orientation \"horiz\" \n                -mergeConnections 0\n                -zoom 1\n                -animateTransition 0\n                -showRelationships 1\n                -showShapes 0\n                -showDeformers 0\n                -showExpressions 0\n                -showConstraints 0\n                -showUnderworld 0\n                -showInvisible 0\n                -transitionFrames 1\n                -opaqueContainers 0\n                -freeform 0\n                -imagePosition 0 0 \n                -imageScale 1\n"
		+ "                -imageEnabled 0\n                -graphType \"DAG\" \n                -heatMapDisplay 0\n                -updateSelection 1\n                -updateNodeAdded 1\n                -useDrawOverrideColor 0\n                -limitGraphTraversal -1\n                -range 0 0 \n                -iconSize \"smallIcons\" \n                -showCachedConnections 0\n                $editorName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"hyperShadePanel\" (localizedPanelLabel(\"Hypershade\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"hyperShadePanel\" -l (localizedPanelLabel(\"Hypershade\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Hypershade\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"visorPanel\" (localizedPanelLabel(\"Visor\")) `;\n"
		+ "\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"visorPanel\" -l (localizedPanelLabel(\"Visor\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Visor\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"polyTexturePlacementPanel\" (localizedPanelLabel(\"UV Texture Editor\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"polyTexturePlacementPanel\" -l (localizedPanelLabel(\"UV Texture Editor\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"UV Texture Editor\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"renderWindowPanel\" (localizedPanelLabel(\"Render View\")) `;\n"
		+ "\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"renderWindowPanel\" -l (localizedPanelLabel(\"Render View\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Render View\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextPanel \"blendShapePanel\" (localizedPanelLabel(\"Blend Shape\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\tblendShapePanel -unParent -l (localizedPanelLabel(\"Blend Shape\")) -mbv $menusOkayInPanels ;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tblendShapePanel -edit -l (localizedPanelLabel(\"Blend Shape\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"dynRelEdPanel\" (localizedPanelLabel(\"Dynamic Relationships\")) `;\n\tif (\"\" == $panelName) {\n"
		+ "\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"dynRelEdPanel\" -l (localizedPanelLabel(\"Dynamic Relationships\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Dynamic Relationships\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"relationshipPanel\" (localizedPanelLabel(\"Relationship Editor\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"relationshipPanel\" -l (localizedPanelLabel(\"Relationship Editor\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Relationship Editor\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"referenceEditorPanel\" (localizedPanelLabel(\"Reference Editor\")) `;\n"
		+ "\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"referenceEditorPanel\" -l (localizedPanelLabel(\"Reference Editor\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Reference Editor\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"componentEditorPanel\" (localizedPanelLabel(\"Component Editor\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"componentEditorPanel\" -l (localizedPanelLabel(\"Component Editor\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Component Editor\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"dynPaintScriptedPanelType\" (localizedPanelLabel(\"Paint Effects\")) `;\n"
		+ "\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"dynPaintScriptedPanelType\" -l (localizedPanelLabel(\"Paint Effects\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Paint Effects\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\t$panelName = `sceneUIReplacement -getNextScriptedPanel \"scriptEditorPanel\" (localizedPanelLabel(\"Script Editor\")) `;\n\tif (\"\" == $panelName) {\n\t\tif ($useSceneConfig) {\n\t\t\t$panelName = `scriptedPanel -unParent  -type \"scriptEditorPanel\" -l (localizedPanelLabel(\"Script Editor\")) -mbv $menusOkayInPanels `;\n\t\t}\n\t} else {\n\t\t$label = `panel -q -label $panelName`;\n\t\tscriptedPanel -edit -l (localizedPanelLabel(\"Script Editor\")) -mbv $menusOkayInPanels  $panelName;\n\t\tif (!$useSceneConfig) {\n\t\t\tpanel -e -l $label $panelName;\n\t\t}\n\t}\n\n\n\tif ($useSceneConfig) {\n        string $configName = `getPanel -cwl (localizedPanelLabel(\"Current Layout\"))`;\n"
		+ "        if (\"\" != $configName) {\n\t\t\tpanelConfiguration -edit -label (localizedPanelLabel(\"Current Layout\")) \n\t\t\t\t-defaultImage \"vacantCell.xP:/\"\n\t\t\t\t-image \"\"\n\t\t\t\t-sc false\n\t\t\t\t-configString \"global string $gMainPane; paneLayout -e -cn \\\"single\\\" -ps 1 100 100 $gMainPane;\"\n\t\t\t\t-removeAllPanels\n\t\t\t\t-ap false\n\t\t\t\t\t(localizedPanelLabel(\"Persp View\")) \n\t\t\t\t\t\"modelPanel\"\n"
		+ "\t\t\t\t\t\"$panelName = `modelPanel -unParent -l (localizedPanelLabel(\\\"Persp View\\\")) -mbv $menusOkayInPanels `;\\n$editorName = $panelName;\\nmodelEditor -e \\n    -cam `findStartUpCamera persp` \\n    -useInteractiveMode 0\\n    -displayLights \\\"default\\\" \\n    -displayAppearance \\\"smoothShaded\\\" \\n    -activeOnly 0\\n    -ignorePanZoom 0\\n    -wireframeOnShaded 0\\n    -headsUpDisplay 1\\n    -selectionHiliteDisplay 1\\n    -useDefaultMaterial 0\\n    -bufferMode \\\"double\\\" \\n    -twoSidedLighting 1\\n    -backfaceCulling 0\\n    -xray 0\\n    -jointXray 0\\n    -activeComponentsXray 0\\n    -displayTextures 0\\n    -smoothWireframe 0\\n    -lineWidth 1\\n    -textureAnisotropic 0\\n    -textureHilight 1\\n    -textureSampling 2\\n    -textureDisplay \\\"modulate\\\" \\n    -textureMaxSize 16384\\n    -fogging 0\\n    -fogSource \\\"fragment\\\" \\n    -fogMode \\\"linear\\\" \\n    -fogStart 0\\n    -fogEnd 100\\n    -fogDensity 0.1\\n    -fogColor 0.5 0.5 0.5 1 \\n    -maxConstantTransparency 1\\n    -rendererName \\\"base_OpenGL_Renderer\\\" \\n    -colorResolution 256 256 \\n    -bumpResolution 512 512 \\n    -textureCompression 0\\n    -transparencyAlgorithm \\\"frontAndBackCull\\\" \\n    -transpInShadows 0\\n    -cullingOverride \\\"none\\\" \\n    -lowQualityLighting 0\\n    -maximumNumHardwareLights 1\\n    -occlusionCulling 0\\n    -shadingModel 0\\n    -useBaseRenderer 0\\n    -useReducedRenderer 0\\n    -smallObjectCulling 0\\n    -smallObjectThreshold -1 \\n    -interactiveDisableShadows 0\\n    -interactiveBackFaceCull 0\\n    -sortTransparent 1\\n    -nurbsCurves 1\\n    -nurbsSurfaces 1\\n    -polymeshes 1\\n    -subdivSurfaces 1\\n    -planes 1\\n    -lights 1\\n    -cameras 1\\n    -controlVertices 1\\n    -hulls 1\\n    -grid 1\\n    -joints 1\\n    -ikHandles 1\\n    -deformers 1\\n    -dynamics 1\\n    -fluids 1\\n    -hairSystems 1\\n    -follicles 1\\n    -nCloths 1\\n    -nParticles 1\\n    -nRigids 1\\n    -dynamicConstraints 1\\n    -locators 1\\n    -manipulators 1\\n    -dimensions 1\\n    -handles 1\\n    -pivots 1\\n    -textures 1\\n    -strokes 1\\n    -shadows 0\\n    $editorName;\\nmodelEditor -e -viewSelected 0 $editorName\"\n"
		+ "\t\t\t\t\t\"modelPanel -edit -l (localizedPanelLabel(\\\"Persp View\\\")) -mbv $menusOkayInPanels  $panelName;\\n$editorName = $panelName;\\nmodelEditor -e \\n    -cam `findStartUpCamera persp` \\n    -useInteractiveMode 0\\n    -displayLights \\\"default\\\" \\n    -displayAppearance \\\"smoothShaded\\\" \\n    -activeOnly 0\\n    -ignorePanZoom 0\\n    -wireframeOnShaded 0\\n    -headsUpDisplay 1\\n    -selectionHiliteDisplay 1\\n    -useDefaultMaterial 0\\n    -bufferMode \\\"double\\\" \\n    -twoSidedLighting 1\\n    -backfaceCulling 0\\n    -xray 0\\n    -jointXray 0\\n    -activeComponentsXray 0\\n    -displayTextures 0\\n    -smoothWireframe 0\\n    -lineWidth 1\\n    -textureAnisotropic 0\\n    -textureHilight 1\\n    -textureSampling 2\\n    -textureDisplay \\\"modulate\\\" \\n    -textureMaxSize 16384\\n    -fogging 0\\n    -fogSource \\\"fragment\\\" \\n    -fogMode \\\"linear\\\" \\n    -fogStart 0\\n    -fogEnd 100\\n    -fogDensity 0.1\\n    -fogColor 0.5 0.5 0.5 1 \\n    -maxConstantTransparency 1\\n    -rendererName \\\"base_OpenGL_Renderer\\\" \\n    -colorResolution 256 256 \\n    -bumpResolution 512 512 \\n    -textureCompression 0\\n    -transparencyAlgorithm \\\"frontAndBackCull\\\" \\n    -transpInShadows 0\\n    -cullingOverride \\\"none\\\" \\n    -lowQualityLighting 0\\n    -maximumNumHardwareLights 1\\n    -occlusionCulling 0\\n    -shadingModel 0\\n    -useBaseRenderer 0\\n    -useReducedRenderer 0\\n    -smallObjectCulling 0\\n    -smallObjectThreshold -1 \\n    -interactiveDisableShadows 0\\n    -interactiveBackFaceCull 0\\n    -sortTransparent 1\\n    -nurbsCurves 1\\n    -nurbsSurfaces 1\\n    -polymeshes 1\\n    -subdivSurfaces 1\\n    -planes 1\\n    -lights 1\\n    -cameras 1\\n    -controlVertices 1\\n    -hulls 1\\n    -grid 1\\n    -joints 1\\n    -ikHandles 1\\n    -deformers 1\\n    -dynamics 1\\n    -fluids 1\\n    -hairSystems 1\\n    -follicles 1\\n    -nCloths 1\\n    -nParticles 1\\n    -nRigids 1\\n    -dynamicConstraints 1\\n    -locators 1\\n    -manipulators 1\\n    -dimensions 1\\n    -handles 1\\n    -pivots 1\\n    -textures 1\\n    -strokes 1\\n    -shadows 0\\n    $editorName;\\nmodelEditor -e -viewSelected 0 $editorName\"\n"
		+ "\t\t\t\t$configName;\n\n            setNamedPanelLayout (localizedPanelLabel(\"Current Layout\"));\n        }\n\n        panelHistory -e -clear mainPanelHistory;\n        setFocus `paneLayout -q -p1 $gMainPane`;\n        sceneUIReplacement -deleteRemaining;\n        sceneUIReplacement -clear;\n\t}\n\n\ngrid -spacing 5 -size 12 -divisions 5 -displayAxes yes -displayGridLines yes -displayDivisionLines yes -displayPerspectiveLabels no -displayOrthographicLabels no -displayAxesBold yes -perspectiveLabelPosition axis -orthographicLabelPosition edge;\nviewManip -drawCompass 0 -compassAngle 0 -frontParameters \"\" -homeParameters \"\" -selectionLockParameters \"\";\n}\n");
	setAttr ".st" 3;
createNode script -n "sceneConfigurationScriptNode";
	setAttr ".b" -type "string" "playbackOptions -min 1 -max 24 -ast 1 -aet 48 ";
	setAttr ".st" 6;
select -ne :time1;
	setAttr ".o" 1;
	setAttr ".unw" 1;
select -ne :renderPartition;
	setAttr -s 4 ".st";
select -ne :initialShadingGroup;
	setAttr ".ro" yes;
select -ne :initialParticleSE;
	setAttr ".ro" yes;
select -ne :defaultShaderList1;
	setAttr -s 4 ".s";
select -ne :defaultTextureList1;
	setAttr -s 2 ".tx";
select -ne :postProcessList1;
	setAttr -s 2 ".p";
select -ne :defaultRenderUtilityList1;
	setAttr -s 2 ".u";
select -ne :renderGlobalsList1;
select -ne :hardwareRenderGlobals;
	setAttr ".ctrs" 256;
	setAttr ".btrs" 512;
select -ne :defaultHardwareRenderGlobals;
	setAttr ".fn" -type "string" "im";
	setAttr ".res" -type "string" "ntsc_4d 646 485 1.333";
connectAttr "cemetery_set_mobile.di" "cemetery_fence_04_corner_left_mobile.do";
relationship "link" ":lightLinker1" ":initialShadingGroup.message" ":defaultLightSet.message";
relationship "link" ":lightLinker1" ":initialParticleSE.message" ":defaultLightSet.message";
relationship "link" ":lightLinker1" "cemetery_fence_04_corner_left_mobileSG.message" ":defaultLightSet.message";
relationship "link" ":lightLinker1" "transpSG.message" ":defaultLightSet.message";
relationship "shadowLink" ":lightLinker1" ":initialShadingGroup.message" ":defaultLightSet.message";
relationship "shadowLink" ":lightLinker1" ":initialParticleSE.message" ":defaultLightSet.message";
relationship "shadowLink" ":lightLinker1" "cemetery_fence_04_corner_left_mobileSG.message" ":defaultLightSet.message";
relationship "shadowLink" ":lightLinker1" "transpSG.message" ":defaultLightSet.message";
connectAttr "layerManager.dli[0]" "defaultLayer.id";
connectAttr "renderLayerManager.rlmi[0]" "defaultRenderLayer.rlid";
connectAttr "cemetery_01_atlas.oc" "cemetery_set_atlas.c";
connectAttr "cemetery_01_atlas.ot" "cemetery_set_atlas.it";
connectAttr "cemetery_set_atlas.oc" "cemetery_fence_04_corner_left_mobileSG.ss";
connectAttr "cemetery_fence_04_corner_left_mobileShape.iog" "cemetery_fence_04_corner_left_mobileSG.dsm"
		 -na;
connectAttr "cemetery_fence_04_corner_left_mobileSG.msg" "materialInfo1.sg";
connectAttr "cemetery_set_atlas.msg" "materialInfo1.m";
connectAttr "cemetery_01_atlas.msg" "materialInfo1.t" -na;
connectAttr "place2dTexture1.o" "cemetery_01_atlas.uv";
connectAttr "place2dTexture1.ofu" "cemetery_01_atlas.ofu";
connectAttr "place2dTexture1.ofv" "cemetery_01_atlas.ofv";
connectAttr "place2dTexture1.rf" "cemetery_01_atlas.rf";
connectAttr "place2dTexture1.reu" "cemetery_01_atlas.reu";
connectAttr "place2dTexture1.rev" "cemetery_01_atlas.rev";
connectAttr "place2dTexture1.vt1" "cemetery_01_atlas.vt1";
connectAttr "place2dTexture1.vt2" "cemetery_01_atlas.vt2";
connectAttr "place2dTexture1.vt3" "cemetery_01_atlas.vt3";
connectAttr "place2dTexture1.vc1" "cemetery_01_atlas.vc1";
connectAttr "place2dTexture1.ofs" "cemetery_01_atlas.fs";
connectAttr "pasted__cemetery_01_atlas.oc" "cemetery_set_atlas_transp.c";
connectAttr "pasted__cemetery_01_atlas.ot" "cemetery_set_atlas_transp.it";
connectAttr "cemetery_set_atlas_transp.oc" "transpSG.ss";
connectAttr "transpSG.msg" "materialInfo2.sg";
connectAttr "cemetery_set_atlas_transp.msg" "materialInfo2.m";
connectAttr "pasted__cemetery_01_atlas.msg" "materialInfo2.t" -na;
connectAttr "place2dTexture2.o" "pasted__cemetery_01_atlas.uv";
connectAttr "place2dTexture2.ofu" "pasted__cemetery_01_atlas.ofu";
connectAttr "place2dTexture2.ofv" "pasted__cemetery_01_atlas.ofv";
connectAttr "place2dTexture2.rf" "pasted__cemetery_01_atlas.rf";
connectAttr "place2dTexture2.reu" "pasted__cemetery_01_atlas.reu";
connectAttr "place2dTexture2.rev" "pasted__cemetery_01_atlas.rev";
connectAttr "place2dTexture2.vt1" "pasted__cemetery_01_atlas.vt1";
connectAttr "place2dTexture2.vt2" "pasted__cemetery_01_atlas.vt2";
connectAttr "place2dTexture2.vt3" "pasted__cemetery_01_atlas.vt3";
connectAttr "place2dTexture2.vc1" "pasted__cemetery_01_atlas.vc1";
connectAttr "place2dTexture2.ofs" "pasted__cemetery_01_atlas.fs";
connectAttr "layerManager.dli[1]" "cemetery_set_mobile.id";
connectAttr "cemetery_fence_04_corner_left_mobileSG.pa" ":renderPartition.st" -na
		;
connectAttr "transpSG.pa" ":renderPartition.st" -na;
connectAttr "cemetery_set_atlas.msg" ":defaultShaderList1.s" -na;
connectAttr "cemetery_set_atlas_transp.msg" ":defaultShaderList1.s" -na;
connectAttr "cemetery_01_atlas.msg" ":defaultTextureList1.tx" -na;
connectAttr "pasted__cemetery_01_atlas.msg" ":defaultTextureList1.tx" -na;
connectAttr "place2dTexture1.msg" ":defaultRenderUtilityList1.u" -na;
connectAttr "place2dTexture2.msg" ":defaultRenderUtilityList1.u" -na;
// End of pillar_small.ma
