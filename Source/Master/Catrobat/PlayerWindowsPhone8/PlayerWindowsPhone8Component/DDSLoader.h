#pragma once

#include <ppltasks.h> 
#include <vector>
#include <string>

#define DDSD_CAPS 0x1
#define DDSD_HEIGHT 0x2
#define DDSD_WIDTH 0x4
#define DDSD_PITCH 0x8
#define DDSD_PIXELFORMAT 0x1000
#define DDSD_HEADERSIZE 124
#define DXGI_FORMAT_R8G8B8A8_TYPELESS 27
#define DDSCAPS_TEXTURE 0x1000

#define DDPF_HEADERSIZE 32
#define DDPF_ALPHAPIXELS 0x1
#define DDPF_RGB 0x40
#define DDPF_AMASK 0xFF000000
#define DDPF_RMASK     0x00FF0000
#define DDPF_GMASK     0x0000FF00
#define DDPF_BMASK     0x000000FF

class DDSLoader
{
public:

	struct DDS_PIXELFORMAT {
	  DWORD dwSize;
	  DWORD dwFlags;
	  DWORD dwFourCC;
	  DWORD dwRGBBitCount;
	  DWORD dwRBitMask;
	  DWORD dwGBitMask;
	  DWORD dwBBitMask;
	  DWORD dwABitMask;
	};

	typedef struct {
	  DWORD           dwSize;
	  DWORD           dwFlags;
	  DWORD           dwHeight;
	  DWORD           dwWidth;
	  DWORD           dwPitchOrLinearSize;
	  DWORD           dwDepth;
	  DWORD           dwMipMapCount;
	  DWORD           dwReserved1[11];
	  DDS_PIXELFORMAT ddspf;
	  DWORD           dwCaps;
	  DWORD           dwCaps2;
	  DWORD           dwCaps3;
	  DWORD           dwCaps4;
	  DWORD           dwReserved2;
	} DDS_HEADER;

	DWORD               dwMagic;	// 'DDS' (0x20534444)
	DDS_HEADER          header;
	BYTE				*bdata;	// We need this for the main surface data
	//BYTE				bdata2[1];	// Other surface data

	DDSLoader(std::vector<unsigned char> image, unsigned int width, unsigned int height);

	void WriteFile();
	void WriteDWord(Windows::Storage::Streams::DataWriter^ state, DWORD dword);

private:
	DDS_HEADER m_ddsHeader;
	DDS_PIXELFORMAT m_ddsPixelformat;
	unsigned int m_streamLength;
	Windows::Storage::StorageFolder^ m_location; 
    Platform::String^ m_locationPath; 
};

