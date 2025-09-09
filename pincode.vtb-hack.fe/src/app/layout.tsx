import React from "react";
import { Geist } from "next/font/google";
import Script from "next/script";
import { viewport } from "@/config/viewport";
import { metadata } from "@/config/metadata";
import { softwareApplicationSchema } from "@/config/schema/software-application";

import "@pin-code/ui-kit/styles";
import "@styles/globals.css";

const geist = Geist({
	subsets: ["latin", "latin-ext"],
});

type Props = {
	children: React.ReactNode;
};

export default async function RootLayout({ children }: Props) {
	return (
		<html className={geist.className} suppressHydrationWarning>
			<head>
				<link rel="canonical" href={metadata.alternates?.canonical as string} />
				<title>{metadata.title as string}</title>

				<Script
					id="software-application-jsonld"
					type="application/ld+json"
					dangerouslySetInnerHTML={{
						__html: JSON.stringify(softwareApplicationSchema),
					}}
				/>
			</head>

			{children}
		</html>
	);
}

export { viewport, metadata };
